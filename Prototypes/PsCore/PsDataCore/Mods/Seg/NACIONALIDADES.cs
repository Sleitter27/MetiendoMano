//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PsDataCore.Mods.Seg
{
    using System;
    using System.Collections.Generic;
    
    public partial class NACIONALIDADES
    {
        public int ID { get; set; }
        public string CODIGO { get; set; }
        public string NOMBRE { get; set; }
        public Nullable<int> PAISID { get; set; }
    
        public virtual PAISES PAISES { get; set; }
    }
}
