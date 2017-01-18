using DevExpress.XtraEditors;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataToSQL
{
    public partial class EqualAddressForm : XtraForm
    {
        public class AddressDef
        {
            public AddressDef(string address)
            {
                Address = address;
            }

            /// <summary>
            /// Имя OPC-переменной.
            /// </summary>
            public string Address { get; set; }
        }

        /// <summary>
        /// Коллекция совпавших адресов.
        /// </summary>
        private Collection<AddressDef> Addresses { get; set; }

        public EqualAddressForm(Dictionary<string, PingServer> equalAddresses)
        {
            InitializeComponent();
            Addresses = new Collection<AddressDef>();

            foreach (var item in equalAddresses)
                Addresses.Add(new AddressDef(item.Key));

            gridControl1.DataSource = Addresses;
        }
    }
}
