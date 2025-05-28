using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using SwiftKampus.Abstractions;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    //[RequireHttps]
    public class BaseController : Controller
    {
        public SchoolDbContext _db;
        public IQueryCommand _query;
        public IStudentQueryManager _studentQuery;
        public bool _IsPayedSchoolFee;
        public bool _IsPayedAcceptance;
        public bool _IsPayedApplicationFee;
        //public bool _IsPayedIdCardFee;
        public ApplicantTypeVm _applicantType;



        public string userId;
        public int semesterId;
        public int sessionId;
        public int studentSchoolProgrammeId;
        public int studentProgrammeId;

        public BaseController(SchoolDbContext db)
        {
            _db = db;
            _query = new QueryCommand(_db);
            _studentQuery = new StudentQueryManager(_db);
            userId = _query.GetId();
            if (!string.IsNullOrEmpty(userId))
            {
                studentSchoolProgrammeId = _query.GetStudentSchoolProgramme(userId);
                studentProgrammeId = _query.GetStudentProgramme(userId);
                semesterId = _query.GetCurrentSemesterId(studentSchoolProgrammeId);
                sessionId = _query.GetCurrentProgrammeSessionId(studentProgrammeId) != 0 ? _query.GetCurrentProgrammeSessionId(studentProgrammeId) : _query.GetCurrentSessionId(studentSchoolProgrammeId);
                var model = _query.GetPaymentStatus(sessionId);

                _IsPayedAcceptance = model.HasPayedAcceptanceFee;
                _IsPayedSchoolFee = model.HasPayedSchoolFee;
                _IsPayedApplicationFee = model.HasPayedApplicationFee;
                _applicantType = new ApplicantTypeVm()
                {
                    ApplicantType = model.SchoolProgramme,
                    TimeType = model.ProgrammeType
                };
            }

        }

        public async Task<SchoolProgramme> CheckUtmeScrenningAvailability()
        {
            return await _db.SchoolProgrammes.AsNoTracking().Where(x => x.ActiveSale.Equals(true)
                                && x.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())).FirstOrDefaultAsync();
        }

        public async Task<SchoolProgramme> CheckUtmeScrenningAvailability(int sessionId)
        {
            return await _db.SchoolProgrammes.AsNoTracking().Where(x => x.ActiveSale.Equals(true) && x.Session.SessionId.Equals((int)sessionId)
                                && x.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())).FirstOrDefaultAsync();
        }

        public (bool, RemitaResponse) CheckExistingTransaction(string checkUrl)
        {
            try
            {
                string jsondata = new WebClient().DownloadString(checkUrl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                return (true, result);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return (false, null);
            }
        }


        public ActionResult ConfirmApplicationFee()
        {
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Applicant))
            {
                if (_IsPayedApplicationFee.Equals(false))
                {
                    return RedirectToAction("Create", "ApplicantPayments");
                }
            }
            return null;
        }

        //public ActionResult ConfirmIdCardPayment()
        //{
        //    if (Request.IsAuthenticated && User.IsInRole(RoleName.Student))
        //    {
        //        if (_IsPayedIdCardFee.Equals(false))
        //        {
        //            return RedirectToAction("Create", "IdCardPayments");
        //        }
        //    }
        //}

        public ActionResult ConfirmAcceptanceFee()
        {
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Student))
            {
                if (_IsPayedAcceptance.Equals(false))
                {
                    return RedirectToAction("MakePayment", "SchoolFeePayments");
                }
            }
            return null;
        }

        public ActionResult ConfirmSchoolFee()
        {
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Student))
            {
                if (_IsPayedSchoolFee.Equals(false))
                {
                    return RedirectToAction("MakePayment", "SchoolFeePayments");
                }
            }
            return null;
        }
        public ActionResult ConfirmSchoolFeeAndAcceptance()
        {
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Student))
            {
                if (_IsPayedSchoolFee.Equals(true) || _IsPayedAcceptance.Equals(true))
                {
                    return null;
                }
            }
            return RedirectToAction("MakePayment", "SchoolFeePayments");
        }

        public async Task<ActionResult> ConfirmProbabtion()
        {
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Student))
            {
                var resultQuery = new ResultCommand(_db);
                var checkProbation = await resultQuery.CheckForProbation(userId);
                if (checkProbation.Item1)
                {
                    return RedirectToAction("ProbabtionError", "Results", new { cgpa = checkProbation.Item2 });
                }
            }
            return null;
        }


        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            if (!string.IsNullOrEmpty(userId))
            {
                var model = _query.GetPaymentStatus(sessionId);
                ViewBag.LayoutViewModel = model;
                ViewBag.FullName = model.FullName;
            }

        }

        public async Task<int> GetUndergraduateSchoolProgrammeId()
        {
            return await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeCode.Equals("UG"))
                                     .Select(s => s.SchoolProgrammeId).FirstOrDefaultAsync();
        }

        public List<string> LevelOrder()
        {
            var levelOrder = new List<string>
            {
                "100","200","300","400","500","600","700","800","900"
            };
            return levelOrder;
        }

        public Dictionary<string, List<string>> PopulateLga()
        {
            var lgaList = new Dictionary<string, List<string>>
            {
                {
                    "FCT",
                    new List<string>()
            {
                "Gwagwalada", "Kuje", "Abaji","Abuja Municipal","Bwari","Kwali", "Others"
            }
                },
                {
                    "ABIA",
                    new List<string>()
            {
                "Aba North","Aba South","Arochukwu","Bende","Ikwuano","Isiala Ngwa North","Isiala Ngwa South","Isuikwuato","Obi Ngwa","Osisiomangwa","Ohafia","Ugwunagbo","Ukwa East","Ukwa West","Umuahia North","Umuahia South","Umu Nneochi"
            }
                },
                {
                    "ADAMAWA",
                    new List<string>()
            {
                "Demsa","Fufore","Ganye","Girie","Maiha","Gayuk","Gombi","Hong","Jada","Lamurde","Madagali","Mayo Belwa","Michika","Mubi North","Mubi South","Numan","Shelleng","Song","Toungo","Yola North","Yola South"
            }
                },
                {
                    "AKWAIBOM",
                    new List<string>()
            {
                "Abak","Eastern Obolo","Eket","Esit Eket","Essien Udim","Etim Ekpo","Etinan","Ibeno","Ibesikpo Asutan","Ibiono-Ibom","Ika","Ikono","Ikot Abasi","Ikot Ekpene","Ini","Itu","Mbo","Mkpat-Enin","Nsit-Atai","Nsit-Ibom","Nsit-Ubium","Obot Akara","Okobo","Onna","Oron","Oruk Anam","Udung-Uko","Ukanafun","Uruan","Urue-Offong Oruko","Uyo"
            }
                },
                {
                    "ANAMBRA",
                    new List<string>()
            {
                "Aguata","Anambra East","Anambra West","Anaocha","Awka North","Awka South","Ayamelum","Dunukofia","Ekwusigo","Idemili North","Idemili South","Ihiala","Njikoka","Nnewi North","Nnewi South","Ogbaru","Onitsha North","Onitsha South","Orumba North","Orumba South","Oyi"
            }
                },
                {
                    "BAUCHI",
                    new List<string>()
            {
                "Alkaleri","Bauchi","Bogoro","Damban","Darazo","Dass","Gamawa","Ganjuwa","Giade","Itas Gadau","Jama'are","Katagum","Kirfi","Misau","Ningi","Shira","Tafawa Balewa","Toro","Warji","Zaki"
            }
                },
                {
                    "BAYELSA",
                    new List<string>()
            {
                "Brass","Ekeremor","Kolokuma Opokuma","Nembe","Ogbia","Sagbama","Southern Ijaw","Yenagoa"
            }
                },
                {
                    "BENUE",
                    new List<string>()
            {
                "Apa","Ado","Agatu","Buruku","Gboko","Guma","Gwer East","Gwer West","Katsina-Ala","Konshisha","Kwande","Logo","Makurdi","Obi","Ogbadibo","Ohimini","Oju","Okpokwu","Oturkpo","Tarka","Ukum","Ushongo","Vandeikya"
            }
                },
                {
                    "BORNO",
                    new List<string>()
            {
                "Abadam","Askira/Uba","Bama","Bayo","Biu","Chibok","Damboa","Dikwa","Gubio","Guzamala","Gwoza","Hawul","Jere","Kaga","Kala/Balge","Konduga","Kukawa","Kwaya Kusar","Mafa","Magumeri","Maiduguri","Marte","Mobbar","Monguno","Ngala","Nganzai","Shani"
            }
                },
                {
                    "CROSSRIVER",
                    new List<string>()
            {
                "Abi","Akamkpa","Akpabuyo","Bakassi","Bekwarra","Biase","Boki","Calabar Municipal","Calabar South","Etung","Ikom","Obanliku","Obubra","Obudu","Odukpani","Ogoja","Yakuur","Yala"
            }
                },

                {
                    "DELTA",
                    new List<string>()
            {
                "Aniocha South","Aniocha North","Bomadi","Burutu","Ethiope East","Ethiope West","Ika North East","Ika South","Isoko North","Isoko South","Ndokwa East","Ndokwa West","Okpe","Oshimili North","Oshimili South","Patani","Sapele","Udu","Ughelli North","Ughelli South","Ukwuani","Uvwie","Warri North","Warri South","Warri South West"
            }
                },
                {
                    "EBONYI",
                    new List<string>()
            {
                "Afikpo North","Afikpo South","Ebonyi","Ezza North","Ezza South","Ikwo","Ishielu","Ivo","Izzi","Ohaozara","Ohaukwu","Onicha"
            }
                },
                {
                    "EDO",
                    new List<string>()
            {
                "Akoko-Edo", "Egor","Esan Central","Esan North-East","Esan South-East","Esan West","Etsako Central","Etsako East","Etsako West","Igueben","Ikpoba Okha","Orhionmwon","Oredo","Ovia North-East","Ovia South-West","Owan East","Owan West","Uhunmwonde"
            }
                },
                {
                    "EKITI",
                    new List<string>()
            {
                "Ado","Efon","Ekiti East","Ekiti South-West","Ekiti West","Emure","Gbonyin","Ido/Osi","Ijero","Ikere","Ikole","Ilejemeje","Irepodun/Ifelodun","Ise Orun","Moba","Oye"
            }
                },

                {
                    "ENUGU",
                    new List<string>()
            {
                "Aninri","Awgu","Enugu East","Enugu North","Enugu South","Ezeagu","Igbo Etiti","Igbo Eze North","Igbo Eze South","Isi Uzo","Nkanu East","Nkanu West","Nsukka","Oji River","Udenu","Udi","Uzo Uwani"
            }
                },
                {
                    "GOMBE",
                    new List<string>()
            {
                "Akko","Balanga","Billiri","Dukku","Funakaye","Gombe","Kaltungo","Kwami","Nafada","Shongom","Yamaltu/Deba"
            }
                },
                {
                    "IMO",
                    new List<string>()
            {
                "Aboh-Mbaise","Ahiazu Mbaise","Ehime Mbano","Ezinihitte","Ideato North","Ideato South","Ihitte/Uboma","Ikeduru","Isiala Mbano","Isu","Mbaitoli","Ngor Okpala","Njaba","Nkwerre","Nwangele","Obowo","Oguta","Ohaji Egbema","Okigwe","Orlu","Orsu","Oru East","Oru West","Owerri Municipal","Owerri North","Owerri West","Unuimo"
            }
                },
                {
                    "JIGAWA",
                    new List<string>()
            {
                "Auyo","Babura","Biriniwa","Birnin Kudu","Buji","Dutse","Gagarawa","Garki","Gumel","Guri","Gwaram","Gwiwa","Hadejia","Jahun","Kafin Hausa","Kazaure","Kiri Kasama","Kiyawa","Kaugama","Maigatari","Malam Madori","Miga","Ringim","Roni","Sule Tankarkar","Taura","Yankwashi"
            }
                },
                {
                    "KADUNA",
                    new List<string>()
            {
                "Birni-Gwari","Chikun","Giwa","Igabi","Ikara","Jaba","Jemaa","Kachia","Kaduna North","Kaduna South","Kagarko","Kajuru","Kaura","Kauru","Kubau","Kudan","Lere","Makarfi","Sabon Gari","Sanga","Soba","Zangon Kataf","Zaria"
            }
                },
                {
                    "KANO",
                    new List<string>()
            {
                "Ajingi","Albasu","Bagwai","Bebeji","Bichi","Bunkure","Dala","Dambatta","Dawakin Kudu","Dawakin Tofa","Doguwa","Fagge","Gabasawa","Garko","Garun Mallam","Gaya","Gezawa","Gwale","Gwarzo","Kabo","Kano Municipal","Karaye","Kibiya","Kiru","Kumbotso","Kunchi","Kura","Madobi","Makoda","Minjibir","Nasarawa","Rano","Rimin Gado","Rogo","Shanono","Sumaila","Takai","Tarauni","Tofa","Tsanyawa","Tudun Wada","Ungogo","Warawa","Wudil"
            }
                },
                {
                    "KATSINA",
                    new List<string>()
            {
                "Bakori","Batagarawa","Batsari","Baure","Bindawa","Charanchi","Dandume","Danja","Dan Musa","Daura","Dutsi","Dutsin Ma","Faskari","Funtua","Ingawa","Jibia","Kafur","Kaita","Kankara","Kankia","Katsina","Kurfi","Kusada","Mai'Adua","Malumfashi","Mani","Mashi","Matazu","Musawa","Rimi","Sabuwa","Safana","Sandamu","Zango"
            }
                },
                {
                    "KEBBI",
                    new List<string>()
            {
                "Aleiro","Arewa Dandi","Argungu","Augie","Bagudo","Birnin Kebbi","Bunza","Dandi","Fakai","Gwandu","Jega","Kalgo","Koko Besse","Maiyama","Ngaski","Sakaba","Shanga","Suru","Wasagu Danko","Yauri","Zuru"
            }
                },
                {
                    "KOGI",
                    new List<string>()
            {
                "Adavi","Ajaokuta","Ankpa","Bassa","Dekina","Ibaji","Idah","Igalamela Odolu","Ijumu","Kabba/Bunu","Kogi","Lokoja","Mopa Muro","Ofu","Ogori Magongo","Okehi","Okene","Olamaboro","Omala","Yagba East","Yagba West"
            }
                },
                {
                    "KWARA",
                    new List<string>()
            {
                "Asa","Baruten","Edu","Ekiti","Ifelodun","Ilorin East","Ilorin South","Ilorin West","Irepodun","Isin","Kaiama","Moro","Offa","Oke Ero","Oyun","Pategi"
            }
                },
                {
                    "LAGOS",
                    new List<string>()
            {
                "Agege","Ajeromi-Ifelodun","Alimosho","Amuwo-Odofin","Apapa","Badagry","Epe","Eti Osa","Ibeju-Lekki","Ifako-Ijaiye","Ikeja","Ikorodu","Kosofe","Lagos Island","Lagos Mainland","Mushin","Ojo","Oshodi-Isolo","Shomolu","Surulere"
            }
                },
                {
                    "NASARAWA",
                    new List<string>()
            {
                "Akwanga","Awe","Doma","Karu","Keana","Keffi","Kokona","Lafia","Nasarawa","Nasarawa Egon","Obi","Toto","Wamba"
            }
                },
                {
                    "NIGER",
                    new List<string>()
            {
                "Agaje","Agwara","Bida","Borgu","Bosso","Chanchaga","Edati","Gbako","Gurara","Katcha","Kontagora","Lapai","Lavun","Magama","Mariga","Mashegu","Mokwa","Moya","Paikoro","Rafi","Rijau","Shiroro","Suleja","Tafa","Wushishi"
            }
                },
                {
                    "OGUN",
                    new List<string>()
            {
                "Abeokuta North","Abeokuta South","Ado-Odo Ota","Egbado North","Egbado South","Ewekoro","Ifo","Ijebu East","Ijebu North","Ijebu North East","Ijebu Ode","Ikenne","Imeko Afon","Ipokia","Obafemi Owode","Odeda","Odogbolu","Ogun Waterside","Remo North","Shagamu"
            }
                },
                {
                    "ONDO",
                    new List<string>()
            {
                "Akoko North-East","Akoko North-West","Akoko South-West","Akoko South-East","Akure North","Akure South","Ese Odo","Idanre","Ifedore","Ilaje","Ile Oluji Okeigbo","Irele","Odigbo","Okitipupa","Ondo East","Ondo West","Ose","Owo"
            }
                },
                {
                    "OSUN",
                    new List<string>()
            {
                "Aiyedade","Aiyedire","Atakumosa East","Atakumosa West","Boluwaduro","Boripe","Ede North","Ede South","Egbedore","Ejigbo","Ife Central","Ife East","Ife North","Ife South","Ifedayo","Ifelodun","Ila","Ilesha East", "Ilesha West", "Irepodun","Irewole","Isokan","Iwo","Obokun","Odo-Otin","Ola-Oluwa","Olarunda","Oriade","Orolu","Osogbo"
            }
                },
                {
                    "OYO",
                    new List<string>()
            {
                "Afijio","Akinyele","Atiba","Atisbo","Egbeda","Ibadan North","Ibadan North-East","Ibadan North-West","Ibadan South-East","Ibadan South-West","Ibarapa Central","Ibarapa East","Ibarapa North","Ido","Irepo","Iseyin","Itesiwaju","Iwajowa","Kajola","Lagelu","Ogbomosho North","Ogbomosho South","Ogo Oluwa","Olorunsogo","Oluyole","Ona Ara","Orelope","Ori Ire","Oyo","Oyo East","Oyo West","Saki East","Saki West","Surulere"
            }
                },
                {
                    "PLATEAU",
                    new List<string>()
            {
                "Barkin Ladi","Bassa","Bokkos","Jos East","Jos North","Jos South","Kanam","Kanke","Langtang South","Langtang North","Mangu","Mikang","Pankshin","Qua'an Pan","Riyom","Shendam","Wase"
            }
                },
                {
                    "RIVERS",
                    new List<string>()
            {
                "Abua/Odual","Ahoada East","Ahoada West","Akuku-Toru","Andoni","Asari-Toru","Bonny","Degema","Eleme","Emuoha","Etche","Gokana","Ikwerre","Khana","Obio Akpor","Ogba Egbema Ndoni","Ogu Bolo","Okrika","Omuma","Opobo Nkoro","Oyigbo","Port Harcourt","Tai"
            }
                },
                {
                    "SOKOTO",
                    new List<string>()
            {
                "Binji","Bodinga","Dange Shuni","Gada","Goronyo","Gudu","Gwadabawa","Illela","Isa","Kebbe","Kware","Rabah","Sabon Birni","Shagari","Silame","Sokoto North","Sokoto South","Tambuwal","Tangaza","Tureta","Wamako","Wurno","Yabo"
            }
                },
                {
                    "TARABA",
                    new List<string>()
            {
                "Ardo-kola","Bali","Donga","Gashaka","Gassol","Ibi","Jalingo","Karim Lamido","Kumi","Lau","Sardauna","Takum","Ussa","Wukari","Yorro","Zing"
            }
                },
                {
                    "YOBE",
                    new List<string>()
            {
                "Bade","Bursari","Damaturu","Fika","Fune","Geidam","Gujba","Gulani","Jakusko","Karasuwa","Machina","Nangere","Nguru","Potiskum","Tarmuwa","Yunusari","Yusufari"
            }
                },
                {
                    "ZAMFARA",
                    new List<string>()
            {
               "Bakura","Birnin Magaji Kiyaw","Bukkuyum","Bungudu","Gummi","Gusau","Kaura Namoda","Maradun","Maru","Shinkafi","Talata Mafara","Tsafe","Zurmi"
            }
                },
                {
                    "FOREIGNER",
                    new List<string>()
            {
               "Foreigner",
            }
                }
            };

            return lgaList;
        }

        public List<Session> GetAllSession()
        {
            return _db.Sessions.OrderByDescending(x => x.SessionName).ToList();
        }

        public List<Semester> GetAllSemesters()
        {
            return _db.Semesters.ToList();
        }

        public List<Programme> GetAllProgramme()
        {
            return _db.Programmes.ToList();
        }

        public List<Course> GetAllCourse()
        {
            return _db.Courses.Include(i => i.Programme).ToList();
        }

        public int GetSessionIdByYear(string year, List<Session> sessions)
        {
            foreach (var item in sessions)
            {
                var splitSessionName = item.SessionName.Split('/', '-');
                if (splitSessionName[0].ToUpper().Equals(year.ToUpper()))
                {
                    return item.SessionId;
                }
            }
            return 0;
        }
        public int GetSessionIdByName(string year, List<Session> sessions)
        {
            var session = sessions.FirstOrDefault(x => x.SessionName.ToUpper().Trim().Equals(year.ToUpper()));
            if (session != null)
            {
                return session.SessionId;
            }
            return 0;
        }

        public int GetSemesterIdByName(string name, List<Semester> semesters)
        {
            var semester = semesters.FirstOrDefault(x => x.SemesterName.ToUpper().Trim().Equals(name.ToUpper()));
            if (semester != null)
            {
                return semester.SemesterId;
            }
            return 0;
        }

        public int GetLevelIdByName(string levelName, List<Level> levels)
        {
            var level = levels.FirstOrDefault(x => x.LevelName.ToUpper().Trim().Equals(levelName.ToUpper()));
            if (level != null)
            {
                return level.LevelId;
            }
            return 0;
        }

        public int GetProgrammeByCode(string programmeCode, List<Programme> programmes)
        {
            var programme = programmes.FirstOrDefault(x => x.ProgrammeCode.ToUpper().Trim().Equals(programmeCode.ToUpper()));
            if (programme != null)
            {
                return programme.ProgrammeId;
            }
            return 0;
        }


        public int GetCourseByCode(string courseCode, List<Course> courses, int programmeId)
        {
            var course = courses.FirstOrDefault(x => x.CourseCode.ToUpper().Trim().Equals(courseCode.ToUpper())
                                && x.Programme.ProgrammeId.Equals(programmeId));
            if (course != null)
            {
                return course.CourseId;
            }
            return 0;
        }

        public List<Level> GetLevelList()
        {
            return _db.Levels.AsNoTracking().ToList();
        }

        public new RedirectToRouteResult RedirectToAction(string action, string controller)
        {
            return base.RedirectToAction(action, controller);
        }

        public DateTime ConvertToDateTime(string passedDateTime)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                DateTime dateTime = DateTime.ParseExact(passedDateTime, new string[] { "yyyy.MM.dd", "yyyy-MM-dd", "yyyy/MM/dd" }, provider, DateTimeStyles.None);
                return dateTime;
            }
            catch (Exception)
            {
                return new DateTime(1990, 1, 18);
            }
        }

        public string GetEmailTemplate()
        {
            string body = string.Empty;

            using (StreamReader reader = new StreamReader(Server.MapPath("~/REmailTemplate.Html")))
            {
                body = reader.ReadToEnd();
            }
            return body;
        }
        public string GetApplicantTemplate()
        {
            string body = string.Empty;

            using (StreamReader reader = new StreamReader(Server.MapPath("~/NEmailTemplate.Html")))
            {
                body = reader.ReadToEnd();
            }
            return body;
        }
        public string GetNotifyApplicantTemplate()
        {
            string body = string.Empty;

            using (StreamReader reader = new StreamReader(Server.MapPath("~/NApplicantTemplate.Html")))
            {
                body = reader.ReadToEnd();
            }
            return body;
        }



        public async Task NotifyByEmail(string userName, string lastName, string firstName, string programmeName, string email, string matricNo, string sessionName, string schoolProgramme)
        {
            string msgBody = GetEmailTemplate();
            msgBody = msgBody.Replace("{STUDENTNAME}", $"{lastName} {firstName}");
            msgBody = msgBody.Replace("{PROGRAMMENAME}", programmeName);
            msgBody = msgBody.Replace("{SESSIONAME}", sessionName);
            msgBody = msgBody.Replace("{EMAILNAME}", email);
            msgBody = msgBody.Replace("{JAMBREGNO}", matricNo);
            msgBody = msgBody.Replace("{SCHOOLPROGRAMME}", schoolProgramme);         


            var emailService = new EmailService();
            await emailService.SendAsync(new IdentityMessage
            {
                Destination = userName,
                Body = msgBody,
                Subject = "Admission Notification Notice"
            });
            //await UserManager.SendEmailAsync(userName, "Notification of Offer of Provisional Admission", msgBody);
        }
        public async Task NotifyApplicantByEmail(string email, string fullName, string header, string body, string formNo)
        {
            string msgBody = GetApplicantTemplate();
            msgBody = msgBody.Replace("{STUDENTNAME}", fullName);
            msgBody = msgBody.Replace("{JAMBREGNO}", formNo);
            msgBody = msgBody.Replace("{BODYMESSAGE}", body);
            msgBody = msgBody.Replace("{MESSAGEHEADER}", header);
            msgBody = msgBody.Replace("{EMAILNAME}", email);


            var emailService = new EmailService();
            await emailService.SendAsync(new IdentityMessage
            {
                Destination = email,
                Body = msgBody,
                Subject = header
            });          
        }
        public async Task NotifyApplicantByEmail(string email, string fullName, string header, string body)
        {
            string msgBody = GetApplicantTemplate();
            msgBody = msgBody.Replace("{STUDENTNAME}", fullName);
            msgBody = msgBody.Replace("{BODYMESSAGE}", body);
            msgBody = msgBody.Replace("{MESSAGEHEADER}", header);
            msgBody = msgBody.Replace("{EMAILNAME}", email);


            var emailService = new EmailService();
            await emailService.SendAsync(new IdentityMessage
            {
                Destination = email,
                Body = msgBody,
                Subject = header
            });
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}