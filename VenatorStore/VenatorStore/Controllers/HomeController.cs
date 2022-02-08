using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VenatorStore.Models;

namespace VenatorStore.Controllers
{
    public class HomeController : Controller
    {
        public StoreContext db;
        private IWebHostEnvironment _appEnvironment;
        private List<string> allRoles = new List<string>() { "admin", "moderator", "user" };

        public HomeController(StoreContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("MyItems", "Home");
            }
            else
            {
                return RedirectToAction("Shop", "Home");
            }
        }

        [HttpGet]
        public IActionResult Registration()
        {
            SetMyGold();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            SetMyGold();
            var foundLogin = db.Users.FirstOrDefault(u => u.Username == user.Username);
            if (foundLogin != null)
            {
                ViewBag.ErrorUser = "Логин " + user.Username + " уже используется!";
                return View();
            }
            var foundEmail = db.Users.FirstOrDefault(u => u.Email == user.Email);
            if (foundEmail != null)
            {
                ViewBag.ErrorUser = "Email " + user.Email + " уже используется!";
                return View();
            }

            AddRoleToUser(user, "user");
            user.DateOfRegistration = DateTime.Now;
            user.Gold = 0;
            db.Users.Add(user);
            db.SaveChanges();

            await Authenticate(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            SetMyGold();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login login)
        {
            SetMyGold();
            if (ModelState.IsValid)
            {
                User user = db.Users.FirstOrDefault(u => u.Username == login.Username && u.Password == login.Password);
                if (user != null)
                {
                    await Authenticate(user);
                    return RedirectToAction("Index", "Home");
                }
                ViewBag.ErrorLogin = login;
                return View();
            }
            else
            {
                ModelState.AddModelError("", "Некоректные данные!");
                return View(login);
            }
        }

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username)
            };
            foreach (var role in GetAllUserRoles(user))
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
            }
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "admin")]
        public IActionResult AllUsers()
        {
            SetMyGold();
            var query = from b in db.Users
                        orderby b.Id
                        select b;

            ViewBag.AllUsers = query;
            ViewBag.AllRoles = allRoles;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult ChangeGold(int id, double newGold)
        {
            User user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.Gold = newGold;
                db.SaveChanges();
            }
            return RedirectToAction("AllUsers", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteUser(int id)
        {
            User user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("AllUsers", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult ChangeRoles(int id, List<string> roles)
        {
            User user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                SetAllUserRoles(user, roles);
                db.SaveChanges();
            }
            return RedirectToAction("AllUsers", "Home");
        }

        [HttpGet]
        public IActionResult Shop(Filter filter)
        {
            SetMyGold();

            var queryHouse = from b in db.Items
                             where b.Category == Category.House && filter.House && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                             orderby b.Id
                             select b;
            var queryShop = from b in db.Items
                            where b.Category == Category.Shop && filter.Shop && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                            orderby b.Id
                            select b;
            var queryHorse = from b in db.Items
                             where b.Category == Category.Horse && filter.Horse && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                             orderby b.Id
                             select b;
            var queryJacket = from b in db.Items
                              where b.Category == Category.Jacket && filter.Jacket && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                              orderby b.Id
                              select b;
            var queryPants = from b in db.Items
                             where b.Category == Category.Pants && filter.Pants && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                             orderby b.Id
                             select b;
            var queryBoots = from b in db.Items
                             where b.Category == Category.Boots && filter.Boots && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                             orderby b.Id
                             select b;
            var querySword = from b in db.Items
                             where b.Category == Category.Sword && filter.Sword && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                             orderby b.Id
                             select b;
            var queryBow = from b in db.Items
                           where b.Category == Category.Bow && filter.Bow && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                           orderby b.Id
                           select b;
            var queryKnife = from b in db.Items
                             where b.Category == Category.Knife && filter.Knife && b.Price >= filter.MinPrice && b.Price <= filter.MaxPrice
                             orderby b.Id
                             select b;

            ViewBag.CountOfProducts = queryHouse.Count() + queryShop.Count() + queryHorse.Count() + queryJacket.Count()
                + queryPants.Count() + queryBoots.Count() + querySword.Count() + queryBow.Count() + queryKnife.Count();

            ViewBag.ItemsHouse = queryHouse.Count() != 0 ? queryHouse : null;
            ViewBag.ItemsShop = queryShop.Count() != 0 ? queryShop : null;
            ViewBag.ItemsHorse = queryHorse.Count() != 0 ? queryHorse : null;
            ViewBag.ItemsJacket = queryJacket.Count() != 0 ? queryJacket : null;
            ViewBag.ItemsPants = queryPants.Count() != 0 ? queryPants : null;
            ViewBag.ItemsBoots = queryBoots.Count() != 0 ? queryBoots : null;
            ViewBag.ItemsSword = querySword.Count() != 0 ? querySword : null;
            ViewBag.ItemsBow = queryBow.Count() != 0 ? queryBow : null;
            ViewBag.ItemsKnife = queryKnife.Count() != 0 ? queryKnife : null;

            ViewBag.Filter = filter;

            return View();
        }

        public IActionResult ClearFilter()
        {
            return RedirectToAction("Shop", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public IActionResult AddToCart(int id)
        {
            User currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (currentUser != null)
            {
                Item item = db.Items.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    List<int> allItemsInCart = DeserializeItems(currentUser.ItemsInCart);
                    allItemsInCart.Add(item.Id);
                    currentUser.ItemsInCart = SerializeItems(allItemsInCart);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Shop", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public IActionResult Buy(int id)
        {
            User currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (currentUser != null)
            {
                Item item = db.Items.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    List<int> allItemsInCart = DeserializeItems(currentUser.ItemsInCart);
                    allItemsInCart.Add(item.Id);
                    currentUser.ItemsInCart = SerializeItems(allItemsInCart);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Cart", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "user")]
        public IActionResult Cart()
        {
            SetMyGold();
            User currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            List<int> allItemsInCartId = DeserializeItems(currentUser.ItemsInCart);
            List<Item> allItemsInCart = new List<Item>();

            foreach (var itemId in allItemsInCartId)
            {
                Item item = db.Items.FirstOrDefault(i => i.Id == itemId);
                allItemsInCart.Add(item);
            }

            ViewBag.CountOfProducts = allItemsInCart.Count;
            ViewBag.TotalCost = 0.0;
            foreach (var item in allItemsInCart)
            {
                ViewBag.TotalCost += item.Price;
            }

            var queryHouse = from b in allItemsInCart
                             where b.Category == Category.House
                             select b;
            var queryShop = from b in allItemsInCart
                            where b.Category == Category.Shop
                            select b;
            var queryHorse = from b in allItemsInCart
                             where b.Category == Category.Horse
                             select b;
            var queryJacket = from b in allItemsInCart
                              where b.Category == Category.Jacket
                              select b;
            var queryPants = from b in allItemsInCart
                             where b.Category == Category.Pants
                             select b;
            var queryBoots = from b in allItemsInCart
                             where b.Category == Category.Boots
                             select b;
            var querySword = from b in allItemsInCart
                             where b.Category == Category.Sword
                             select b;
            var queryBow = from b in allItemsInCart
                           where b.Category == Category.Bow
                           select b;
            var queryKnife = from b in allItemsInCart
                             where b.Category == Category.Knife
                             select b;

            ViewBag.ItemsHouse = queryHouse.Count() != 0 ? queryHouse : null;
            ViewBag.ItemsShop = queryShop.Count() != 0 ? queryShop : null;
            ViewBag.ItemsHorse = queryHorse.Count() != 0 ? queryHorse : null;
            ViewBag.ItemsJacket = queryJacket.Count() != 0 ? queryJacket : null;
            ViewBag.ItemsPants = queryPants.Count() != 0 ? queryPants : null;
            ViewBag.ItemsBoots = queryBoots.Count() != 0 ? queryBoots : null;
            ViewBag.ItemsSword = querySword.Count() != 0 ? querySword : null;
            ViewBag.ItemsBow = queryBow.Count() != 0 ? queryBow : null;
            ViewBag.ItemsKnife = queryKnife.Count() != 0 ? queryKnife : null;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public IActionResult DeleteItemFromCart(int id)
        {
            User currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            List<int> allItemsInCart = DeserializeItems(currentUser.ItemsInCart);

            for (int i = 0; i < allItemsInCart.Count; i++)
            {
                Item item = db.Items.FirstOrDefault(itm => itm.Id == allItemsInCart[i]);
                if (item.Id == id)
                {
                    allItemsInCart.RemoveAt(i);
                    break;
                }
            }

            currentUser.ItemsInCart = SerializeItems(allItemsInCart);
            db.SaveChanges();
            return RedirectToAction("Cart", "Home");
        }

        [Authorize(Roles = "user")]
        public IActionResult PurchaseConfirmation(int code)
        {
            User currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            List<int> allItemsInCartId = DeserializeItems(currentUser.ItemsInCart);
            List<Item> allItemsInCart = new List<Item>();
            foreach (var itemId in allItemsInCartId)
            {
                Item item = db.Items.FirstOrDefault(i => i.Id == itemId);
                allItemsInCart.Add(item);
            }

            double totalCost = 0.0;
            foreach (var item in allItemsInCart)
            {
                totalCost += item.Price;
            }
            ViewBag.TotalCost = totalCost;
            ViewBag.CountOfProducts = allItemsInCart.Count;

            if (allItemsInCart.Count == 0)
            {
                return RedirectToAction("Shop", "Home");
            }

            int _code = (DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString()).GetHashCode();
            if (_code == code)
            {
                if (currentUser.Gold - totalCost >= 0.0)
                {
                    currentUser.Gold -= totalCost;
                    List<int> items = DeserializeItems(currentUser.Items);
                    items.AddRange(DeserializeItems(currentUser.ItemsInCart));
                    currentUser.Items = SerializeItems(items);
                    currentUser.ItemsInCart = SerializeItems(new List<int>());
                    db.SaveChanges();
                    ViewBag.Form = 2;
                    SetMyGold();
                    return View();
                }
                return RedirectToAction("PurchaseConfirmation", "Home");
            }

            if (currentUser.Gold - totalCost < 0) // недостаточно золота
            {
                ViewBag.DifferenceInGold = totalCost - currentUser.Gold;
                ViewBag.Form = 0;
            }
            else
            {
                ViewBag.Form = 1;
            }

            SetMyGold();
            return View();
        }

        [Authorize(Roles = "user")]
        public IActionResult Confirmed()
        {
            int _code = (DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString()).GetHashCode();
            return RedirectToAction("PurchaseConfirmation", "Home", new { code = _code });
        }

        [Authorize(Roles = "user")]
        public IActionResult MyItems()
        {
            SetMyGold();
            User currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            List<int> allItemsId = DeserializeItems(currentUser.Items);
            List<Item> allItems = new List<Item>();

            foreach (var itemId in allItemsId)
            {
                Item item = db.Items.FirstOrDefault(i => i.Id == itemId);
                allItems.Add(item);
            }

            ViewBag.CountOfProducts = allItems.Count;

            var queryHouse = from b in allItems
                             where b.Category == Category.House
                             select b;
            var queryShop = from b in allItems
                            where b.Category == Category.Shop
                            select b;
            var queryHorse = from b in allItems
                             where b.Category == Category.Horse
                             select b;
            var queryJacket = from b in allItems
                              where b.Category == Category.Jacket
                              select b;
            var queryPants = from b in allItems
                             where b.Category == Category.Pants
                             select b;
            var queryBoots = from b in allItems
                             where b.Category == Category.Boots
                             select b;
            var querySword = from b in allItems
                             where b.Category == Category.Sword
                             select b;
            var queryBow = from b in allItems
                           where b.Category == Category.Bow
                           select b;
            var queryKnife = from b in allItems
                             where b.Category == Category.Knife
                             select b;

            ViewBag.ItemsHouse = queryHouse.Count() != 0 ? queryHouse : null;
            ViewBag.ItemsShop = queryShop.Count() != 0 ? queryShop : null;
            ViewBag.ItemsHorse = queryHorse.Count() != 0 ? queryHorse : null;
            ViewBag.ItemsJacket = queryJacket.Count() != 0 ? queryJacket : null;
            ViewBag.ItemsPants = queryPants.Count() != 0 ? queryPants : null;
            ViewBag.ItemsBoots = queryBoots.Count() != 0 ? queryBoots : null;
            ViewBag.ItemsSword = querySword.Count() != 0 ? querySword : null;
            ViewBag.ItemsBow = queryBow.Count() != 0 ? queryBow : null;
            ViewBag.ItemsKnife = queryKnife.Count() != 0 ? queryKnife : null;

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult AllItems(string error)
        {
            SetMyGold();

            var queryHouse = from b in db.Items
                             where b.Category == Category.House
                             orderby b.Id
                             select b;
            var queryShop = from b in db.Items
                            where b.Category == Category.Shop
                            orderby b.Id
                            select b;
            var queryHorse = from b in db.Items
                             where b.Category == Category.Horse
                             orderby b.Id
                             select b;
            var queryJacket = from b in db.Items
                              where b.Category == Category.Jacket
                              orderby b.Id
                              select b;
            var queryPants = from b in db.Items
                             where b.Category == Category.Pants
                             orderby b.Id
                             select b;
            var queryBoots = from b in db.Items
                             where b.Category == Category.Boots
                             orderby b.Id
                             select b;
            var querySword = from b in db.Items
                             where b.Category == Category.Sword
                             orderby b.Id
                             select b;
            var queryBow = from b in db.Items
                           where b.Category == Category.Bow
                           orderby b.Id
                           select b;
            var queryKnife = from b in db.Items
                             where b.Category == Category.Knife
                             orderby b.Id
                             select b;

            ViewBag.ItemsHouse = queryHouse.Count() != 0 ? queryHouse : null;
            ViewBag.ItemsShop = queryShop.Count() != 0 ? queryShop : null;
            ViewBag.ItemsHorse = queryHorse.Count() != 0 ? queryHorse : null;
            ViewBag.ItemsJacket = queryJacket.Count() != 0 ? queryJacket : null;
            ViewBag.ItemsPants = queryPants.Count() != 0 ? queryPants : null;
            ViewBag.ItemsBoots = queryBoots.Count() != 0 ? queryBoots : null;
            ViewBag.ItemsSword = querySword.Count() != 0 ? querySword : null;
            ViewBag.ItemsBow = queryBow.Count() != 0 ? queryBow : null;
            ViewBag.ItemsKnife = queryKnife.Count() != 0 ? queryKnife : null;

            ViewBag.Error = error;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AllItems(Item item, IFormFile image)
        {
            string imageName = DateTime.Now.GetHashCode().ToString() + new Regex(@"\W").Replace(Path.GetFileNameWithoutExtension(image.FileName), "") + Path.GetExtension(image.FileName);
            string path = "/img/items/" + item.Category + "/" + imageName;
            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            try
            {
                using (Image img = Image.FromFile(_appEnvironment.WebRootPath + path)) { }
            }
            catch
            {
                return RedirectToAction("AllItems", "Home", new { error = "*файл не является изображением!" });
            }

            item.ImageUrl = imageName;
            db.Items.Add(item);
            db.SaveChanges();
            return RedirectToAction("AllItems", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteItem(int id)
        {
            Item item = db.Items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                db.Items.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("AllItems", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult ChangePrice(int id, double newPrice)
        {
            Item item = db.Items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.Price = newPrice;
                db.SaveChanges();
            }
            return RedirectToAction("AllItems", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Роли и Json
        public void AddRoleToUser(User user, string role)
        {
            if (allRoles.Contains(role))
            {
                List<string> userRoles = GetAllUserRoles(user);
                userRoles.Add(role);
                user.Roles = SerializeRoles(userRoles);
            }
        }

        public void RemoveRoleFromUser(User user, string role)
        {
            List<string> userRoles = GetAllUserRoles(user);
            if (userRoles.Contains(role))
            {
                userRoles.Remove(role);
                user.Roles = SerializeRoles(userRoles);
            }
        }

        public List<string> GetAllUserRoles(User user)
        {
            return DeserializeRoles(user.Roles);
        }

        public void SetAllUserRoles(User user, List<string> roles)
        {
            user.Roles = SerializeRoles(roles);
        }

        private string SerializeRoles(List<string> roles)
        {
            return JsonConvert.SerializeObject(roles);
        }

        private List<string> DeserializeRoles(string roles)
        {
            return JsonConvert.DeserializeObject<List<string>>(roles);
        }

        private string SerializeItems(List<int> items)
        {
            return JsonConvert.SerializeObject(items);
        }

        private List<int> DeserializeItems(string items)
        {
            return JsonConvert.DeserializeObject<List<int>>(items);
        }
        #endregion

        private void SetMyGold()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewData["MyGold"] = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name).Gold;
            }
        }
    }
}