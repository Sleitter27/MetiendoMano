//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PsDataCore.Mods.Pos
{
    using System;
    using System.Collections.Generic;
    
    public partial class CUENTAS_CLASIFICACION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CUENTAS_CLASIFICACION()
        {
            this.CUENTAS = new HashSet<CUENTAS>();
        }
    
        public int ID { get; set; }
        public string CODIGO { get; set; }
        public string NOMBRE { get; set; }
        public string DESCRIPCION { get; set; }
        public Nullable<decimal> BALANCE { get; set; }
        public Nullable<int> CUENTAS_ORIGENID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CUENTAS> CUENTAS { get; set; }
        public virtual CUENTAS_ORIGEN CUENTAS_ORIGEN { get; set; }
    }
}
