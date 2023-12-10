using m21_e2_WEB.Models;
using m21_e2_WEB.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace m21_e2_WEB.Controllers
{
    public class HomeController : Controller //контроллер управления главной страницей приложения
    {
        public HttpClient httpClient { get; set; }
        public IWebHostEnvironment webHostEnvironment { get; set; }
        private readonly SignInManager<User> _signInManager;
        public HomeController(IWebHostEnvironment webHostEnvironment, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            httpClient = new HttpClient();
            this.webHostEnvironment = webHostEnvironment;
        }

        public DateTime ChangeTime(DateTime dateTime, int hours, int minutes, int seconds = default, int milliseconds = default)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hours, minutes, seconds, milliseconds, dateTime.Kind);
        }

        //метод выгрузки текстов сайта
        public MainClass GetSiteTexts()
        {
            //тексты страницы
            string urlText = @"https://localhost:44380/api/application/sitetext/";
            string jsonText = httpClient.GetStringAsync(urlText).Result;

            //случайный слоган
            string urlTagLine = @"https://localhost:44380/api/application/randomtagline/";
            string jsonTagLine = httpClient.GetStringAsync(urlTagLine).Result;

            //коллекция слоганов
            string urlTagLineList = @"https://localhost:44380/api/application/tagline/";
            string jsonTagLineList = httpClient.GetStringAsync(urlTagLineList).Result;

            MainClass mc = new MainClass();
            mc.siteText = JsonConvert.DeserializeObject<SiteText>(jsonText);
            mc.tagLine = JsonConvert.DeserializeObject<TagLine>(jsonTagLine);
            mc.tagLineList = JsonConvert.DeserializeObject<List<TagLine>>(jsonTagLineList);

            return mc;
        }

        //метод подсчёта заявок
        public int RequestCounter(List<Request> requests, DateTime inputDateFrom, DateTime InputDateTo)
        {
            var dateFrom = ChangeTime(inputDateFrom, 0, 0, 0, 0);
            var dateTo = ChangeTime(InputDateTo, 0, 0, 0, 0);
            var newReqs = new List<Request>();
            foreach (Request req in requests)
            {
                if (req.RequestTime <= dateTo && req.RequestTime >= dateFrom)
                {
                    newReqs.Add(req);
                }
            }
            return newReqs.Count;
        }

        //метод подсчёта процента заявок
        public string GetRequestPercent(int reqsAllTime, int reqsPeriod)
        {
            if (reqsAllTime != 0 && reqsPeriod != 0)
            {
                string percentReqsToday = (Convert.ToDouble(reqsPeriod) / Convert.ToDouble(reqsAllTime) * 100).ToString("#.##");
                return percentReqsToday + "%";
            }
            else
            {
                return "0";
            }
        }

        //ГЛАВНАЯ СТРАНИЦА------------------------------------------------------------
        //главная страница
        public IActionResult MainPage()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //страница редактирования главной страницы
        public IActionResult EditMainPage()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //сохранение текстов с главной страницы
        [HttpPost]
        public IActionResult EditMainPageConfirm(MainClass text)
        {
            //тексты
            SiteText st = text.siteText;

            //слоганы
            List<TagLine> tl = text.tagLineList;
            tl[0].Id = 1;
            tl[1].Id = 2;
            tl[2].Id = 3;
            text.tagLineList = tl;

            string urlText = @"https://localhost:44380/api/application/sitetext/";
            var result = httpClient.PutAsync(
                requestUri: urlText,
                content: new StringContent(JsonConvert.SerializeObject(text), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("MainPage", "Home");
        }

        //РАБОЧИЙ СТОЛ
        public IActionResult WorkPage(MainClass mc)
        {
            if (mc.requestPeriod == null)
            {
                mc = GetSiteTexts();
                string url1 = @"https://localhost:44380/api/application/request/";
                string json1 = httpClient.GetStringAsync(url1).Result;
                mc.requestsList = JsonConvert.DeserializeObject<List<Request>>(json1);
                ViewBag.reqsAllTime = mc.requestsList.Count;
                mc.requestsList = null;
                return View(mc);
            }
            var dateFrom = ChangeTime(mc.requestPeriod.DateFrom, 0, 0, 0, 0);
            var dateTo = ChangeTime(mc.requestPeriod.DateTo, 0, 0, 0, 0);           
            mc = GetSiteTexts();
            mc.requestPeriod = new RequestPeriod();
            mc.requestPeriod.DateFrom = dateFrom;
            mc.requestPeriod.DateTo = dateTo;

            string url = @"https://localhost:44380/api/application/request/";
            string json = httpClient.GetStringAsync(url).Result;
            mc.requestsList = JsonConvert.DeserializeObject<List<Request>>(json);            
            ViewBag.reqsAllTime = mc.requestsList.Count; //всего заявок            
            ViewBag.reqsPeriod = RequestCounter(mc.requestsList, mc.requestPeriod.DateFrom, mc.requestPeriod.DateTo); //за период
            ViewBag.percentPeriod = GetRequestPercent(ViewBag.reqsAllTime, ViewBag.reqsPeriod);
            if (mc.requestPeriod == null)
            {                
                return View(mc);
            }
            List<Request> requests = new List<Request>();
            mc.requestPeriod.DateFrom = ChangeTime(mc.requestPeriod.DateFrom, 0, 0, 0, 0);
            mc.requestPeriod.DateTo = ChangeTime(mc.requestPeriod.DateTo, 0, 0, 0, 0);
            foreach (Request req in mc.requestsList)
            {
                if (req.RequestTime <= mc.requestPeriod.DateTo && req.RequestTime >= mc.requestPeriod.DateFrom)
                {
                    requests.Add(req);
                }
            }
            mc.requestsList = requests;
            return View(mc);
        }
        //СТРАНИЦА ОШИБКИ
        public IActionResult ErrorPage()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //ЗАЯВКИ------------------------------------------------------------
        //создание заявки с главной страницы
        [HttpPost]
        public IActionResult CreateRequest(MainClass request)
        {
            string url = @"https://localhost:44380/api/application/request/";
            RequestViewModel req = new RequestViewModel 
            {
                RequesterName = request.requestViewModel.RequesterName,
                RequestEmail = request.requestViewModel.RequestEmail,
                RequestText = request.requestViewModel.RequestText
            };
            var result = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("RequestAccept", "Home");
        }

        //принятие заявки
        public IActionResult RequestAccept()
        {
            var mc = GetSiteTexts();
            return View(mc);            
        }

        //редактировать статус заявки
        [HttpPost]
        public IActionResult EditRequestStatus(MainClass mc)
        {            
            Request req = mc.request;
            string url = @"https://localhost:44380/api/application/request/";
            var result = httpClient.PutAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            var dateFrom = mc.requestPeriod.DateFrom;
            var dateTo = mc.requestPeriod.DateTo;
            mc = GetSiteTexts();
            mc.requestPeriod = new RequestPeriod();
            mc.requestPeriod.DateFrom = dateFrom;
            mc.requestPeriod.DateTo = dateTo;
            string json = httpClient.GetStringAsync(url).Result;
            mc.requestsList = JsonConvert.DeserializeObject<List<Request>>(json);            
            ViewBag.reqsAllTime = mc.requestsList.Count; //всего заявок           
            ViewBag.reqsPeriod = RequestCounter(mc.requestsList, mc.requestPeriod.DateFrom, mc.requestPeriod.DateTo); //за период
            ViewBag.percentPeriod = GetRequestPercent(ViewBag.reqsAllTime, ViewBag.reqsPeriod);
            List<Request> requests = new List<Request>();
            mc.requestPeriod.DateFrom = ChangeTime(mc.requestPeriod.DateFrom, 0, 0, 0, 0);
            mc.requestPeriod.DateTo = ChangeTime(mc.requestPeriod.DateTo, 0, 0, 0, 0);
            foreach (Request reqs in mc.requestsList)
            {
                if (reqs.RequestTime <= mc.requestPeriod.DateTo && reqs.RequestTime >= mc.requestPeriod.DateFrom)
                {
                    requests.Add(reqs);
                }
            }
            mc.requestsList = requests;
            return View("WorkPage",mc);
        }

        //УСЛУГИ------------------------------------------------------------
        //страница перечня услуг
        public IActionResult ServicesPage()
        {
            string urlServices = @"https://localhost:44380/api/application/services/";
            string jsonServices = httpClient.GetStringAsync(urlServices).Result;
            var mc = GetSiteTexts();
            mc.servicesList = JsonConvert.DeserializeObject<IEnumerable<Service>>(jsonServices);
            return View(mc);
        }

        //страница добавления услуги
        public IActionResult AddServicePage()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //добавление услуги
        [HttpPost]
        public IActionResult AddService(MainClass mcservice)
        {
            string url = @"https://localhost:44380/api/application/services/";
            Service newServ = new Service
            {
                ServiceTitle = mcservice.service.ServiceTitle,
                ServiceDescription = mcservice.service.ServiceDescription
            };
            var result = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(newServ), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("ServicesPage", "Home");
        }

        //удалить услугу
        [HttpPost]
        public async Task<ActionResult> DeleteService(string id)
        {
            string url = @$"https://localhost:44380/api/application/services/{id}";
            var result = await httpClient.DeleteAsync(url);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("ServicesPage", "Home");
        }

        //страница редактирования услуги
        public IActionResult EditServicePage(int? id)
        {
            string url = @$"https://localhost:44380/api/application/services/{id}";
            string json = httpClient.GetStringAsync(url).Result;
            var mc = GetSiteTexts();
            mc.service = JsonConvert.DeserializeObject<Service>(json);            
            return View(mc);
        }

        //редактирование услуги
        [HttpPost]
        public IActionResult EditService(Service service)
        {
            string url = @"https://localhost:44380/api/application/services/";
            var result = httpClient.PutAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("ServicesPage", "Home");
        }

        //ПРОЕКТЫ------------------------------------------------------------
        //страница перечня проектов
        public IActionResult ProjectsPage()
        {
            string urlProjects = @"https://localhost:44380/api/application/projects/";
            string jsonProjects = httpClient.GetStringAsync(urlProjects).Result;
            var mc = GetSiteTexts();
            mc.projectsList = JsonConvert.DeserializeObject<IEnumerable<Project>>(jsonProjects);
            return View(mc);
        }

        //страница проекта по Id
        public IActionResult ProjectDetailPage(int id)
        {
            string urlProject = $@"https://localhost:44380/api/application/projects/{id}";
            string jsonProject = httpClient.GetStringAsync(urlProject).Result;
            var mc = GetSiteTexts();
            mc.project = JsonConvert.DeserializeObject<Project>(jsonProject);
            return View(mc);
        }

        //страница добавления проекта
        public IActionResult AddProjectPage()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //добавить проект
        public async Task<IActionResult> AddProject([FromForm] MainClass mcproject)
        {
            if (mcproject.projectViewModel.ProjectTitle == null || mcproject.projectViewModel.ProjectDescription == null || mcproject.projectViewModel.PictureFile == null)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            string url = @"https://localhost:44380/api/application/projects/";
            byte[] data;
            using (var br = new BinaryReader(mcproject.projectViewModel.PictureFile.OpenReadStream()))
            {
                data = br.ReadBytes((int)mcproject.projectViewModel.PictureFile.OpenReadStream().Length);
            }
            ByteArrayContent bytes = new ByteArrayContent(data);
            MultipartFormDataContent multiContent = new MultipartFormDataContent();
            multiContent.Add(bytes, "PictureFile", mcproject.projectViewModel.PictureFile.FileName);
            multiContent.Add(new StringContent(mcproject.projectViewModel.ProjectTitle), "ProjectTitle");
            multiContent.Add(new StringContent(mcproject.projectViewModel.ProjectDescription), "ProjectDescription");
            var result = await httpClient.PostAsync(url, multiContent);
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("ProjectsPage", "Home");
        }

        //удалить проект
        [HttpPost]
        public async Task<ActionResult> DeleteProject(string id)
        {
            string url = @$"https://localhost:44380/api/application/projects/{id}";
            var result = await httpClient.DeleteAsync(url);
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("ProjectsPage", "Home");
        }

        //страница редактирования проекта
        public IActionResult EditProjectPage(int? id)
        {
            string url = @$"https://localhost:44380/api/application/projects/{id}";
            string json = httpClient.GetStringAsync(url).Result;
            var mc = GetSiteTexts();
            mc.project = JsonConvert.DeserializeObject<Project>(json);
            return View(mc);
        }

        //редактировать проект
        [HttpPost]
        public async Task<IActionResult> EditProject([FromForm] MainClass mcproject)
        {
            MultipartFormDataContent multiContent = new MultipartFormDataContent();
            multiContent.Add(new StringContent(mcproject.project.Id.ToString()), "Id");
            multiContent.Add(new StringContent(mcproject.projectViewModel.ProjectTitle), "ProjectTitle");
            multiContent.Add(new StringContent(mcproject.project.ProjectDescription), "ProjectDescription");
            if(mcproject.projectViewModel.PictureFile == null)
            {
                string url = @"https://localhost:44380/api/application/projectsnonepix/";
                var result = await httpClient.PutAsync(url, multiContent);
                                
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }
                return RedirectToAction("ProjectsPage", "Home");
            }
            if (mcproject.projectViewModel.PictureFile != null)
            {
                string url = @"https://localhost:44380/api/application/projects/";
                byte[] data;
                using (var br = new BinaryReader(mcproject.projectViewModel.PictureFile.OpenReadStream()))
                {
                    data = br.ReadBytes((int)mcproject.projectViewModel.PictureFile.OpenReadStream().Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                multiContent.Add(bytes, "PictureFile", mcproject.projectViewModel.PictureFile.FileName);
                var result = await httpClient.PutAsync(url, multiContent);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }
                return RedirectToAction("ProjectsPage", "Home");
            }
            return RedirectToAction("ErrorPage", "Home");
        }

        //БЛОГ------------------------------------------------------------
        //страница перечня блога
        public IActionResult BlogPage()
        {
            string urlBlogs = @"https://localhost:44380/api/application/blog/";
            string jsonBlogs = httpClient.GetStringAsync(urlBlogs).Result;
            var mc = GetSiteTexts();
            mc.blogList = JsonConvert.DeserializeObject<IEnumerable<Blog>>(jsonBlogs);
            return View(mc);
        }

        //страница блога по Id
        public IActionResult BlogDetailPage(int id)
        {
            string urlBlog = $@"https://localhost:44380/api/application/blog/{id}";
            string jsonBlog = httpClient.GetStringAsync(urlBlog).Result;
            var mc = GetSiteTexts();
            mc.blog = JsonConvert.DeserializeObject<Blog>(jsonBlog);
            return View(mc);
        }

        //страница добавления блога
        public IActionResult AddBlogPage()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }
       
        //добавить блог
        public async Task<IActionResult> AddBlog([FromForm] MainClass mcblog)
        {
            if (mcblog.blogViewModel.BlogTitle == null || mcblog.blogViewModel.BlogDescription == null || mcblog.blogViewModel.PictureFile == null)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            string url = @"https://localhost:44380/api/application/blog/";
            byte[] data;
            using (var br = new BinaryReader(mcblog.blogViewModel.PictureFile.OpenReadStream()))
            {
                data = br.ReadBytes((int)mcblog.blogViewModel.PictureFile.OpenReadStream().Length);
            }
            ByteArrayContent bytes = new ByteArrayContent(data);
            MultipartFormDataContent multiContent = new MultipartFormDataContent();
            multiContent.Add(bytes, "PictureFile", mcblog.blogViewModel.PictureFile.FileName);
            multiContent.Add(new StringContent(mcblog.blogViewModel.BlogTitle), "BlogTitle");
            multiContent.Add(new StringContent(mcblog.blogViewModel.BlogDescription), "BlogDescription");
            var result = await httpClient.PostAsync(url, multiContent);            
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("BlogPage", "Home");
        }

        //удалить блог
        [HttpPost]
        public async Task<ActionResult> DeleteBlog(string id)
        {
            string url = @$"https://localhost:44380/api/application/blog/{id}";
            var result = await httpClient.DeleteAsync(url);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("BlogPage", "Home");
        }

        //страница редактирования блога
        public IActionResult EditBlogPage(int? id)
        {
            string url = @$"https://localhost:44380/api/application/blog/{id}";
            string json = httpClient.GetStringAsync(url).Result;
            var mc = GetSiteTexts();
            mc.blog = JsonConvert.DeserializeObject<Blog>(json);
            return View(mc);
        }

        //редактировать блог
        [HttpPost]
        public async Task<IActionResult> EditBlog([FromForm] MainClass mcblog)
        {
            MultipartFormDataContent multiContent = new MultipartFormDataContent();
            multiContent.Add(new StringContent(mcblog.blog.Id.ToString()), "Id");
            multiContent.Add(new StringContent(mcblog.blogViewModel.BlogTitle), "BlogTitle");
            multiContent.Add(new StringContent(mcblog.blog.BlogDescription), "BlogDescription");            
            if (mcblog.blogViewModel.PictureFile == null)
            {
                string url = @"https://localhost:44380/api/application/blognonepix/";
                var result = await httpClient.PutAsync(url, multiContent);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }
                return RedirectToAction("BlogPage", "Home");
            }
            if (mcblog.blogViewModel.PictureFile != null)
            {
                string url = @"https://localhost:44380/api/application/blog/";
                byte[] data;
                using (var br = new BinaryReader(mcblog.blogViewModel.PictureFile.OpenReadStream()))
                {
                    data = br.ReadBytes((int)mcblog.blogViewModel.PictureFile.OpenReadStream().Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                multiContent.Add(bytes, "PictureFile", mcblog.blogViewModel.PictureFile.FileName);

                var result = await httpClient.PutAsync(url, multiContent);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }
                return RedirectToAction("BlogPage", "Home");
            }
            return RedirectToAction("ErrorPage", "Home");
        }

        //КОНТАКТЫ------------------------------------------------------------
        //страница с контактами
        public IActionResult ContactsPage()
        {
            string urlContacts = @"https://localhost:44380/api/application/contacts/";
            string jsonContacts = httpClient.GetStringAsync(urlContacts).Result;
            var mc = GetSiteTexts();
            mc.contactsList = JsonConvert.DeserializeObject<List<Contact>>(jsonContacts);
            return View(mc);
        }

        //страница редактирования ссылок контактов
        public IActionResult EditContactsLinksPage()
        {
            string urlContacts = @"https://localhost:44380/api/application/contacts/";
            string jsonContacts = httpClient.GetStringAsync(urlContacts).Result;
            var mc = GetSiteTexts();
            mc.contactsList = JsonConvert.DeserializeObject<List<Contact>>(jsonContacts);
            return View(mc);
        }

        //редактирвоание ссылок контактов
        [HttpPost]
        public async Task<IActionResult> EditContactsLinksPageConfirm([FromForm] MainClass mccontacts)
        {   
            MultipartFormDataContent multiContent = new MultipartFormDataContent();
            multiContent.Add(new StringContent(mccontacts.contact.Id.ToString()), "Id");
            multiContent.Add(new StringContent(mccontacts.contact.ContactText), "ContactText");
            multiContent.Add(new StringContent(mccontacts.contact.ContactLink), "ContactLink");
            if (mccontacts.contactViewModel == null)
            {
                string url = @"https://localhost:44380/api/application/contactnonepix/";
                var result = await httpClient.PutAsync(url, multiContent);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }
                return RedirectToAction("EditContactsLinksPage", "Home");
            }
            if (mccontacts.contactViewModel != null)
            {
                string url = @"https://localhost:44380/api/application/contactwithpix/";
                byte[] data;
                using (var br = new BinaryReader(mccontacts.contactViewModel.PictureFile.OpenReadStream()))
                {
                    data = br.ReadBytes((int)mccontacts.contactViewModel.PictureFile.OpenReadStream().Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                multiContent.Add(bytes, "PictureFile", mccontacts.contactViewModel.PictureFile.FileName);
                var result = await httpClient.PutAsync(url, multiContent);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }
                return RedirectToAction("EditContactsLinksPage", "Home");
            }
            return RedirectToAction("ErrorPage", "Home");
        }

        //удалить ссылку на контакт
        [HttpPost]
        public async Task<ActionResult> DeleteContact(string id)
        {
            string url = @$"https://localhost:44380/api/application/contacts/{id}";
            var result = await httpClient.DeleteAsync(url);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("EditContactsLinksPage", "Home");
        }

        //добавить ссылку на контакт
        public async Task<IActionResult> AddContact([FromForm] MainClass mccontact)
        {
            string url = @"https://localhost:44380/api/application/contacts/";
            byte[] data;
            using (var br = new BinaryReader(mccontact.contactViewModel.PictureFile.OpenReadStream()))
            {
                data = br.ReadBytes((int)mccontact.contactViewModel.PictureFile.OpenReadStream().Length);
            }
            ByteArrayContent bytes = new ByteArrayContent(data);
            MultipartFormDataContent multiContent = new MultipartFormDataContent();
            multiContent.Add(bytes, "PictureFile", mccontact.contactViewModel.PictureFile.FileName);
            multiContent.Add(new StringContent(mccontact.contact.ContactText), "ContactText");
            multiContent.Add(new StringContent(mccontact.contact.ContactLink), "ContactLink");
            var result = await httpClient.PostAsync(url, multiContent);
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("EditContactsLinksPage", "Home");
        }

        //страница редактирования текста контактов
        public IActionResult EditContactsTextsPage()
        {
            string urlContacts = @"https://localhost:44380/api/application/contacts/";
            string jsonContacts = httpClient.GetStringAsync(urlContacts).Result;
            var mc = GetSiteTexts();
            mc.contactsList = JsonConvert.DeserializeObject<List<Contact>>(jsonContacts);
            return View(mc);
        }

        //сохранение текстов страницы контактов
        [HttpPost]
        public IActionResult EditContactsTextsPageConfirm(MainClass text)
        {            
            SiteText st = text.siteText;

            string urlText = @"https://localhost:44380/api/application/contactstexts/";
            var result = httpClient.PutAsync(
                requestUri: urlText,
                content: new StringContent(JsonConvert.SerializeObject(text), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
            return RedirectToAction("ContactsPage", "Home");
        }
                        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        //страница регистрации
        public IActionResult Register()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //регистрация
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(MainClass mcin)
        {
            RegisterViewModel model = mcin.registerViewModel;
            var mc = GetSiteTexts();
            if (!ModelState.IsValid)
            {
                User newUser = new User
                {
                    UserName = model.UserName
                };
                UserForm reg = new UserForm()
                {
                    UserName = model.UserName,
                    Password = model.Password
                };
                string url = @"https://localhost:44380/api/application/register";
                var result = await httpClient.PostAsync(
                    requestUri: url,
                    content: new StringContent(JsonConvert.SerializeObject(reg), Encoding.UTF8,
                    mediaType: "application/json")
                    );

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    await _signInManager.SignInAsync(newUser, false);
                    return RedirectToAction("MainPage", "Home");
                }
                else
                {
                    return RedirectToAction("ErrorCreateAccount", "Home");
                }
            }
            return View(mc);
        }

        //страница логина
        [HttpGet]
        public IActionResult Login()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //логин
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(MainClass mcin)
        {
            LoginViewModel model = mcin.loginViewModel;
            var mc = GetSiteTexts();
            if (!ModelState.IsValid)
            {
                UserForm log = new UserForm()
                {
                    UserName = model.UserName,
                    Password = model.Password
                };
                User user = new User()
                {
                    UserName = log.UserName
                };                
                string url = @"https://localhost:44380/api/application/login";
                var result = await httpClient.PostAsync(
                    requestUri: url,
                    content: new StringContent(JsonConvert.SerializeObject(log), Encoding.UTF8,
                    mediaType: "application/json")
                    );
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("MainPage", "Home");
                }
                else
                {
                    return RedirectToAction("ErrorLoginAccount", "Home");
                }
            }
            return View(mc);
        }

        //логаут
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("MainPage", "Home");
        }

        //страница запрета доступа
        public IActionResult AccessDenied()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //страница ошибки при регистрации
        public IActionResult ErrorCreateAccount()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }

        //страница ошибки при логине
        public IActionResult ErrorLoginAccount()
        {
            var mc = GetSiteTexts();
            return View(mc);
        }
    }
}