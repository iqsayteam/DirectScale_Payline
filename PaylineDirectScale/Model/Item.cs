namespace PaylineDirectScale.Model
{
    public class Item
    {
        private string name;
        private string price;
        private string quantity;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
            }
        }

        public string Price
        {
            get { return this.price; }
            set
            {
                this.price = value;
            }
        }

        public string Quantity
        {
            get { return this.quantity; }
            set
            {
                this.quantity = value;
            }
        }
    }
}
