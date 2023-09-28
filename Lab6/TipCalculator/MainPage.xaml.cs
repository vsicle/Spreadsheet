namespace TipCalculator
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        double input;
        double tipInput;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            TipAmount.Text = (input * (tipInput / 100)).ToString();
            TotalAmount.Text = (input * (tipInput / 100) + input).ToString();
            SemanticScreenReader.Announce(FindTipBtn.Text);
        }

        private void OnBillAmountChanged(object sender, EventArgs e)
        {
            if (double.TryParse(BillAmount.Text, out _))
            {
                FindTipBtn.IsEnabled = true;
                input = double.Parse(BillAmount.Text);
            }
            else
            {
                FindTipBtn.IsEnabled = false;
            }
            
        }

        private void OnTipInputChanged(object sender, EventArgs e)
        {
            if (double.TryParse(TipInput.Text, out _))
            {
                FindTipBtn.IsEnabled = true;
                tipInput = double.Parse(TipInput.Text);
            }
            else
            {
                FindTipBtn.IsEnabled = false;
            }
            
        }
    }
}