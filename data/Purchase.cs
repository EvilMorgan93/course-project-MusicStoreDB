//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MusicStoreDB_App.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Purchase
    {
        public int id_purchase { get; set; }
        public int id_album { get; set; }
        public int id_employee { get; set; }
        public int purchase_amount { get; set; }
        public Nullable<long> purchase_number { get; set; }
        public System.DateTime purchase_date { get; set; }
    
        public virtual Album Album { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
