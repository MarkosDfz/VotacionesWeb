using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.IO;
using System.Linq;
using System.Web;
using votaciones.Models;
using System.Security.Cryptography;
using System.Text;

namespace votaciones.Classes
{
    public class Utilities : IDisposable
    {
        private static DemocracyContext db = new DemocracyContext();

        public static string UploadPhoto(HttpPostedFileBase file)
        {
            //Subir Imagen

            string path = string.Empty;
            string pic = string.Empty;

            if (file != null)
            {
                pic = Path.GetFileName(file.FileName);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/Security/Content/Photos"), pic);
                file.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            return pic;
        }

        public static void CreateASPUser(UserView userView)
        {
            //Gestion de usuarios

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            //crear el rol del usuario

            string roleName = "User";

            //comprobar que el rol no existe sino lo creamos

            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }

            // creamos el usuario de ASPNET

            var userASP = new ApplicationUser
            {
                UserName = userView.Cedula,
                Email = userView.Cedula,
            };

            userManager.Create(userASP, userASP.UserName);

            userASP = userManager.FindByName(userView.Cedula);
            userManager.AddToRole(userASP.Id, "User");

        }

        public static void DeleteASPUser(string id)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var user = (from p in userContext.Users
                           where p.Email == id
                           select p).FirstOrDefault();

            userManager.Delete(user);

        }

        public static void ChangeUserName(string currentUserName, UserChange user)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.FindByEmail(currentUserName);
            if (userASP == null)
            {
                return;
            }

            userManager.Delete(userASP);

            userASP = new ApplicationUser
            {
                UserName = user.Cedula,
                Email = user.Cedula,
            };

            userManager.Create(userASP, user.CurrentPassword);
            userManager.AddToRole(userASP.Id, "User");
        }

        public static State GetState(string stateName)
        {
            var state = db.States.Where(s => s.Description == stateName).FirstOrDefault();
            if (state == null)
            {
                state = new State
                {
                    Description = stateName,
                };

                db.States.Add(state);
                db.SaveChanges();
            }

            return state;
        }

        public static string Encriptar(string texto)
        {
            try
            {

                string key = "democracyurjosemejiainc"; //llave para encriptar datos

                byte[] keyArray;

                byte[] Arreglo_a_Cifrar = UTF8Encoding.UTF8.GetBytes(texto);

                //Se utilizan las clases de encriptación MD5

                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));

                hashmd5.Clear();

                //Algoritmo TripleDES
                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateEncryptor();

                byte[] ArrayResultado = cTransform.TransformFinalBlock(Arreglo_a_Cifrar, 0, Arreglo_a_Cifrar.Length);

                tdes.Clear();

                //se regresa el resultado en forma de una cadena
                texto = Convert.ToBase64String(ArrayResultado, 0, ArrayResultado.Length);

            }
            catch (Exception)
            {

            }
            return texto;
        }

        public static string Desencriptar(string textoEncriptado)
        {
            try
            {
                string key = "democracyurjosemejiainc";
                byte[] keyArray;
                byte[] Array_a_Descifrar = Convert.FromBase64String(textoEncriptado);

                //algoritmo MD5
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));

                hashmd5.Clear();

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();

                byte[] resultArray = cTransform.TransformFinalBlock(Array_a_Descifrar, 0, Array_a_Descifrar.Length);

                tdes.Clear();
                textoEncriptado = UTF8Encoding.UTF8.GetString(resultArray);

            }
            catch (Exception)
            {

            }
            return textoEncriptado;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}