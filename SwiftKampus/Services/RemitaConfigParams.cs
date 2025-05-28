namespace SwiftKampus.Services
{
    public static class RemitaConfigParams
    {
        /* Life Credentials */
        public const string UTILITY = "526474601";
        public const string UTMEAPPLICANTION = "3078840427";
        public const string ACCOMODATIONSERVICETYPE = "1651079028";
        public const string HOSTELAPPLICATIONSERVICETYPE = "1651079028"; //1520647591
        public const string CHANGEOFCOURSE = "526474601";
        public const string ACCEPTANCESERVICETYPE = "1519043034";
        public const string PGACCEPTANCESERVICETYPE = "1520647591";
        public const string PGSCHOOLFEESERVICETYPE = "1520647591";
        public const string SCHOOLFEESERVICETYPE = "1520647591";
        public const string SUPPLEMENTARYSERVICETYPE = "3078840427";




        public const string MERCHANTID = "540814763";
        public const string APIKEY = "142368";
       // public const string CHECKSTATUSURL = "https://login.remita.net/remita/ecomm";
        public const string CHECKSTATUSURL = "https://login.remita.net/remita/exapp/api/v1/send/api/echannelsvc";

        /* Life Credentials */


        /* Demo Credentials */
        //public const string MERCHANTID = "2547916";
        //public const string APIKEY = "1946";
        //public const string CHECKSTATUSURL = "https://www.remitademo.net/remita/ecomm";

        //public const string SCHOOLFEESERVICETYPE = "4430731";
        //public const string ACCEPTANCESERVICETYPE = "4430731";
        //public const string SUPPLEMENTARYSERVICETYPE = "4430731";
        //public const string HOSTELAPPLICATIONSERVICETYPE = "4430731";
        //public const string ACCOMODATIONSERVICETYPE = "4430731";
        //public const string UTMEAPPLICANTION = "4430731";
        /* Demo Credentials */
    }

    public class RemitaRePostVm
    {
        public string merchantId { get; set; }
        public string hash { get; set; }
        public string rrr { get; set; }
        public string responseurl { get; set; }
    }
}