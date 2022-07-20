using Microsoft.AspNetCore.Mvc;
using System;

namespace WebApiFingertech2._0.Controllers
{
    [Controller]
    public class ConvertController : ControllerBase
    {

        [HttpGet]
        [Route("Capturar/{id:int:min(1)}")]
        public string Capturar(int id)
        {
            return Entity.FingerClass.Capture(id);
        }

        [HttpGet]
        [Route("Enroll/{id:int:min(1)}")]
        public string Enroll(int id)
        {
            return Entity.FingerClass.Enroll(id);
        }

        [HttpGet]
        //[Route("Identificar/{id:int:min(1)}")]
        [Route("Identificar")]
        public string Identificar(String digital)
        {
            return Entity.FingerClass.Identificar(digital);
        }

        [HttpGet]
        [Route("Comparar")]
        public string Comparar(String digital)
        {
            return Entity.FingerClass.Comparar(digital);
        }

        //Convert RAW to WSQ below.

        [HttpGet]
        [Route("Converter")]
        public string ConvertWsq()
        {
            return Entity.FingerClass.Converter();
        }
    }
}
