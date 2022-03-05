using cursomvcapi.Models.WS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using cursomvcapi.Models;

namespace cursomvcapi.Controllers
{
    public class AccessController : ApiController
    {
        [HttpGet]
        public Reply HelloWorld()
        {
            Reply oR = new Reply();
            oR.result = 1;
            oR.message = "My World";

            return oR;
        }

        [HttpPost]
        public Reply Login([FromBody] AccessViewModel model)
        {
            Reply oR = new Reply();
            try
            {
                using (cursomvcapiEntities db = new cursomvcapiEntities())
                {
                    var lst = db.user.Where(d => d.email == model.email && d.password == model.password && d.idEstatus == 1);

                    if (lst.Count() > 0)
                    {
                        oR.result = 1;
                        oR.data = Guid.NewGuid().ToString();
                        oR.message = "Login exitoso";

                        user oUser = lst.First();
                        oUser.token = (string) oR.data;
                        db.Entry(oUser).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {
                        oR.message = "Datos errores";
                    }
                }
            }
            catch 
            {
                oR.result = 0;
                oR.message = "Ocurrió un error en el Servidor";
            }

            return oR;
        }
    }
}
