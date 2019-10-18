﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using MusicStoreDB_App.Commands;
using MusicStoreDB_App.Data;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MusicStoreDB_App.ViewModels {
    public class PurchaseViewModel : BaseViewModel, IPageViewModel {
        public CollectionViewSource Purchase { get; private set; }
        public CollectionViewSource Album { get; private set; }
        public CollectionViewSource Employee { get; private set; }
        public CollectionViewSource Price { get; private set; }
        public string Name {
            get => "Продажи";
        }
        private Purchase selectedItem;
        public Purchase SelectedItem {
            get => selectedItem;
            set {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
                ButtonAddContent = "Добавить";
            }
        }

        public PurchaseViewModel() {
            Purchase = new CollectionViewSource();
            Album = new CollectionViewSource();
            Employee = new CollectionViewSource();
            Price = new CollectionViewSource();
            RefreshData();
            SelectedItem = Purchase.View.CurrentItem as Purchase;
            ButtonAddContent = "Добавить";
            SaveEvent = new SaveCommand(this);
            AddEvent = new AddCommand(this);
            RefreshEvent = new RefreshCommand(this);
            DeleteEvent = new DeleteCommand(this);
            ExportEvent = new ExportPurchasesCommand(this);
        }
        public void ExportPucrhasesToPDF() {
            try {
                var document = new Document();
                var writer = PdfWriter.GetInstance(document, new FileStream("Отчёт по продажам.pdf", FileMode.Create));
                document.Open();
                using (var dbContext = new MusicStoreDBEntities()) {
                    string ttf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIAL.TTF");
                    var baseFont = BaseFont.CreateFont(ttf, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                    var font = new Font(baseFont, Font.DEFAULTSIZE, Font.NORMAL);
                    string[] nameColumns = new string[] {
                        "Имя продавца",
                        "Название альбома",
                        "Название группы",
                        "Дата покупки",
                        "Цена"
                    };
                    var table = new PdfPTable(nameColumns.Length);
                    PdfPCell cell = new PdfPCell(new Phrase("Отчёт по продажам", font)) {
                        Colspan = nameColumns.Length,
                        HorizontalAlignment = 1,
                        Border = 0,
                        PaddingBottom = 10
                    };
                    table.AddCell(cell);
                    var query = (from a in dbContext.Albums
                                 join g in dbContext.Groups on a.id_artist equals g.id_artist
                                 join p in dbContext.Purchases on a.id_album equals p.id_album
                                 join pr in dbContext.Price_List on p.id_price equals pr.id_price
                                 join emp in dbContext.Employees on p.id_employee equals emp.id_employee into ps
                                 from emp in ps.DefaultIfEmpty()
                                 select new {
                                     emp.employee_name,
                                     a.album_name,
                                     g.group_name,
                                     p.purchase_date,
                                     pr.purchase_price
                                 }).ToList();
                    for (int i = 0; i < nameColumns.Length; i++) {
                        cell = new PdfPCell(new Phrase(nameColumns[i], font)) {
                            BackgroundColor = BaseColor.LIGHT_GRAY,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 3,
                        };
                        table.AddCell(cell);
                    }
                    for (int k = 0; k < query.Count; k++) {
                        table.AddCell(new PdfPCell(new Phrase(query[k].employee_name, font)) {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });
                        table.AddCell(new PdfPCell(new Phrase(query[k].album_name, font)) {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });
                        table.AddCell(new PdfPCell(new Phrase(query[k].group_name, font)) {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });
                        table.AddCell(new PdfPCell(new Phrase(query[k].purchase_date.ToString("G"), font)) {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });
                        table.AddCell(new PdfPCell(new Phrase(query[k].purchase_price.ToString(), font)) {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });
                    }
                    document.Add(table);
                }
                document.Close();
                writer.Close();
                MessageBox.Show("Отчёт сформирован!", "Информация об отчёте", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Информация об отчёте", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void RefreshData() {
            using (var dbContext = new MusicStoreDBEntities()) {
                Purchase.Source = dbContext.Purchases.ToList();
                var employeeQuery = (from emp in dbContext.Employees
                             select new {
                                 emp.id_employee,
                                 emp.employee_name
                             }).ToList();
                Employee.Source = employeeQuery;
                var albumQuery = (from a in dbContext.Albums
                                     select new {
                                         a.id_album,
                                         a.album_name
                                     }).ToList();
                Album.Source = albumQuery;
                var priceQuery = (from p in dbContext.Price_List
                                     select new {
                                         p.id_price,
                                         p.purchase_price
                                     }).ToList();
                Price.Source = priceQuery;
            }
        }
        public void SaveChanges() {
            try {
                using (var dbContext = new MusicStoreDBEntities()) {
                    if (ButtonAddContent == "Отмена") {
                        AddPurchaseData(dbContext);
                        ButtonAddContent = "Добавить";
                    } else {
                        EditPurchaseData(dbContext);
                    }
                }
                RefreshData();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
        public void AddPurchaseData(MusicStoreDBEntities dbContext) {
            dbContext.Purchases.Add(SelectedItem);
            dbContext.SaveChanges();
        }
        public void EditPurchaseData(MusicStoreDBEntities dbContext) {
            dbContext.Entry(SelectedItem).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
        public void DeletePurchaseData() {
            try {
                using (var dbContext = new MusicStoreDBEntities()) {
                    var entity = SelectedItem;
                    dbContext.Entry(entity).State = EntityState.Deleted;
                    dbContext.SaveChanges();
                    RefreshData();
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
