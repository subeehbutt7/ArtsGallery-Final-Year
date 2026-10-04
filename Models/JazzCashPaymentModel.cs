namespace ArtGalleryFinal.Models
{
    // Fields required by JazzCash Hosted Checkout (HTTP POST) integration
    public class JazzCashPaymentModel
    {
        public string pp_Version { get; set; } = "1.1";
        public string pp_TxnType { get; set; } = "MWALLET";
        public string pp_Language { get; set; } = "EN";
        public string pp_MerchantID { get; set; } = string.Empty;
        public string pp_SubMerchantID { get; set; } = "";
        public string pp_Password { get; set; } = string.Empty;
        public string pp_BankID { get; set; } = "";
        public string pp_ProductID { get; set; } = "";
        public string pp_TxnRefNo { get; set; } = string.Empty;
        public string pp_Amount { get; set; } = string.Empty; // amount in paisa (Rs * 100)
        public string pp_TxnCurrency { get; set; } = "PKR";
        public string pp_TxnDateTime { get; set; } = string.Empty;
        public string pp_BillReference { get; set; } = string.Empty; // we store OrderId here
        public string pp_Description { get; set; } = "Art Gallery Order Payment";
        public string pp_TxnExpiryDateTime { get; set; } = string.Empty;
        public string pp_ReturnURL { get; set; } = string.Empty;
        public string pp_SecureHash { get; set; } = string.Empty;
    }
}
