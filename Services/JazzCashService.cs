using ArtGalleryFinal.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ArtGalleryFinal.Services
{
    public class JazzCashService : IJazzCashService
    {
        private readonly IConfiguration _config;

        public JazzCashService(IConfiguration config)
        {
            _config = config;
        }

        // Simulation mode is ON if the developer hasn't put real Merchant ID/Password/Salt
        // in appsettings.json yet (see "JazzCash" section), or if explicitly set to true.
        public bool IsSimulationMode =>
            string.IsNullOrWhiteSpace(_config["JazzCash:MerchantId"]) ||
            _config.GetValue<bool>("JazzCash:SimulationMode", true);

        public string GatewayUrl =>
            _config["JazzCash:SandboxUrl"] ?? "https://sandbox.jazzcash.com.pk/CustomerPortal/transactionmanagement/merchantform/";

        public JazzCashPaymentModel BuildPaymentRequest(int orderId, decimal amount)
        {
            var model = new JazzCashPaymentModel
            {
                pp_MerchantID = _config["JazzCash:MerchantId"] ?? "",
                pp_Password = _config["JazzCash:Password"] ?? "",
                pp_TxnRefNo = "T" + DateTime.Now.Ticks,
                pp_Amount = ((long)(amount * 100)).ToString(), // JazzCash expects amount in paisa
                pp_TxnDateTime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                pp_TxnExpiryDateTime = DateTime.Now.AddHours(1).ToString("yyyyMMddHHmmss"),
                pp_BillReference = orderId.ToString(),
                pp_ReturnURL = _config["JazzCash:ReturnUrl"] ?? ""
            };

            model.pp_SecureHash = GenerateSecureHash(model, _config["JazzCash:IntegritySalt"] ?? "");
            return model;
        }

        // JazzCash requires an HMAC-SHA256 hash of the sorted field values, prefixed with the Integrity Salt
        private string GenerateSecureHash(JazzCashPaymentModel m, string integritySalt)
        {
            var sortedFields = new SortedDictionary<string, string>
            {
                { "pp_Amount", m.pp_Amount },
                { "pp_BillReference", m.pp_BillReference },
                { "pp_Description", m.pp_Description },
                { "pp_Language", m.pp_Language },
                { "pp_MerchantID", m.pp_MerchantID },
                { "pp_Password", m.pp_Password },
                { "pp_ReturnURL", m.pp_ReturnURL },
                { "pp_TxnCurrency", m.pp_TxnCurrency },
                { "pp_TxnDateTime", m.pp_TxnDateTime },
                { "pp_TxnExpiryDateTime", m.pp_TxnExpiryDateTime },
                { "pp_TxnRefNo", m.pp_TxnRefNo },
                { "pp_TxnType", m.pp_TxnType },
                { "pp_Version", m.pp_Version }
            };

            string combined = integritySalt + "&" + string.Join("&", sortedFields.Values);

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(integritySalt));
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
