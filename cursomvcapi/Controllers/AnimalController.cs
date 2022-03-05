using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using cursomvcapi.Models.WS;
using cursomvcapi.Models;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Text;

namespace cursomvcapi.Controllers
{
    public class AnimalController : BaseController
    {
        [HttpPost]
        public Reply Get([FromBody] SecurityViewModel model)
        {
            Reply oR = new Reply();
            oR.result = 0;

            if (!Verify(model.token))
            {
                oR.message = "No autorizado";
                return oR;
            }            

            try
            {
                using (cursomvcapiEntities db = new cursomvcapiEntities())
                {
                    List<ListAnimalsViewModel> lst = (from d in db.animal
                                                      where d.idState == 1
                                                      select new ListAnimalsViewModel
                                                      {
                                                          Name = d.name,
                                                          Patas = d.patas
                                                      }).ToList();
                    oR.data = lst;
                    oR.result = 1;
                }
            }
            catch
            {
                oR.message = "Ocurrió un error en el Servidor";
            }

            return oR;
        }

        [HttpPost]
        public Reply Add([FromBody]AnimalViewModel model)
        {
            Reply oR = new Reply();
            oR.result = 0;

            if (!Verify(model.token))
            {
                oR.message = "No autorizado";
                return oR;
            }

            if (!Validate(model))
            {
                oR.message = error;
                return oR;
            }

            try
            {
                using (cursomvcapiEntities db = new cursomvcapiEntities())
                {
                    animal oAnimal = new animal();
                    oAnimal.idState = 1;
                    oAnimal.name = model.Name;
                    oAnimal.patas = model.Patas;

                    db.animal.Add(oAnimal);
                    db.SaveChanges();

                    List<ListAnimalsViewModel> lst = List(db);
                    oR.result = 1;
                    oR.data = lst;
                }
            }
            catch
            {
                oR.message = "Ocurrió un error en el Servidor";
            }
            return oR;
        }

        [HttpPut]
        public Reply Edit([FromBody] AnimalViewModel model)
        {
            Reply oR = new Reply();
            oR.result = 0;

            if (!Verify(model.token))
            {
                oR.message = "No autorizado";
                return oR;
            }

            try
            {
                using (cursomvcapiEntities db = new cursomvcapiEntities())
                {
                    animal oAnimal = db.animal.Find(model.Id);                    
                    oAnimal.name = model.Name;
                    oAnimal.patas = model.Patas;

                    db.Entry(oAnimal).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    List<ListAnimalsViewModel> lst = List(db);
                    oR.result = 1;
                    oR.data = lst;
                }
            }
            catch
            {
                oR.message = "Ocurrió un error en el Servidor";
            }
            return oR;
        }

        [HttpDelete]
        public Reply Delete([FromBody] AnimalViewModel model)
        {
            Reply oR = new Reply();
            oR.result = 0;

            if (!Verify(model.token))
            {
                oR.message = "No autorizado";
                return oR;
            }

            if (!Validate(model))
            {
                oR.message = error;
                return oR;
            }

            try
            {
                using (cursomvcapiEntities db = new cursomvcapiEntities())
                {
                    animal oAnimal = db.animal.Find(model.Id);
                    oAnimal.idState = 2;

                    db.Entry(oAnimal).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    List<ListAnimalsViewModel> lst = List(db);
                    oR.result = 1;
                    oR.data = lst;
                }
            }
            catch
            {
                oR.message = "Ocurrió un error en el Servidor";
            }
            return oR;
        }

        [HttpPost]
        public async Task<Reply> Photo([FromUri] AnimalPictureViewModel model)
        {
            Reply oR = new Reply();
            oR.result = 0;

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            if (!Verify(model.token))
            {
                oR.message = "No autorizado";
                return oR;
            }

            if (!Request.Content.IsMimeMultipartContent())
            {
                oR.message = "No viene imagen";
                return oR;
            }

            await Request.Content.ReadAsMultipartAsync(provider);

            FileInfo fileInfoPicture = null;

            foreach (MultipartFileData fileData in provider.FileData)
            {
                if (fileData.Headers.ContentDisposition.Name.Replace("\\", "").Replace("\"","").Equals("picture"))
                    fileInfoPicture = new FileInfo(fileData.LocalFileName);
            }

            if (fileInfoPicture != null)
            {
                using (FileStream fs = fileInfoPicture.Open(FileMode.Open, FileAccess.Read))
                {
                    byte[] b = new byte[fileInfoPicture.Length];
                    UTF8Encoding temp = new UTF8Encoding(true);
                    while (fs.Read(b, 0, b.Length) > 0);

                    try
                    {
                        using (cursomvcapiEntities db = new cursomvcapiEntities())
                        {
                            var oAnimal = db.animal.Find(model.Id);
                            oAnimal.picture = b;
                            db.Entry(oAnimal).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            oR.result = 1;
                        }
                    }
                    catch
                    {
                        oR.message = "Ocurrió un error en el Servidor";
                    }

                }
            }

            return oR;                
        }

        private bool Validate(AnimalViewModel model)
        {
            if (model.Name == "")
            {
                error = "El nombre es obligatorio";
                return false;
            }
            return true;    
        }

        private List<ListAnimalsViewModel> List(cursomvcapiEntities db)
        {
            List<ListAnimalsViewModel> lst = (from d in db.animal
             where d.idState == 1
             select new ListAnimalsViewModel
             {
                 Name = d.name,
                 Patas = d.patas
             }).ToList();
            return lst;
        }
    }
}
