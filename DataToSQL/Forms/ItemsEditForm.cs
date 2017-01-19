using DevExpress.Xpo;
using DevExpress.XtraEditors;

namespace DataToSQL
{
    public partial class ItemsEditForm : XtraForm
    {
        public ItemsEditForm(XPCollection itemCollection)
        {
            InitializeComponent();
            ItemCollection = itemCollection;
        }

        /// <summary>
        /// Ссылка на коллекцию элементов.
        /// </summary>
        XPCollection ItemCollection;

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            foreach (Item item in ItemCollection)
                item.TimeOut = (int)spTimeOut.Value;
        }

        private void ItemsEditForm_Load(object sender, System.EventArgs e)
        {
            if (ItemCollection.Count > 0)
                spTimeOut.Value = ((Item)ItemCollection[0]).TimeOut;
        }
    }
}
