using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using mercadopago;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace WebApplication2
{
    public partial class receive_ipn : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            MP mp = new MP("7071654091217780", "F4SUQfv2CA4YUvPj0VsFROGywMkcYvyC");

            mp.sandboxMode(false); // cambiar por mp.sandboxMode(false);
            string Nombre_de_archivo = Request["id"].ToString();
            
            JObject payment_info = mp.getPaymentInfo(Nombre_de_archivo);

            if (Nombre_de_archivo.Length == 9) // trato de capturar la respuesta y sacarla del juego enviada desde MP
            {
                Response.Write(payment_info["response"]);
                return;
            }
            else // trato de capturar la primera respuesta y trabajarla
            { 

                //Preparando para enviar al correo electrónico
                MailMessage correo = new MailMessage();
                correo.From = new MailAddress("correodelosprofesores@gmail.com");
                correo.To.Add("Licenciados@outlook.com.ar");


                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587;

                smtp.Credentials = new NetworkCredential("correodelosprofesores@gmail.com", "qsoiqzuliwweyeog");
           
                smtp.EnableSsl = true;
                
                string cuerpo = "";

                string valorar = "";

                string mensaje = payment_info["response"].ToString();

                string reemplazo_llave_abierto = mensaje.Replace("{", "");
                string reemplazo_llave_cerrado = reemplazo_llave_abierto.Replace("}", "");
                string reemplazo_comillas = reemplazo_llave_cerrado.Replace("\"", "");
                string reemplazo_espacios = reemplazo_comillas.Replace(" ", "");

                string[] descomponer = reemplazo_comillas.Split(',');

                string[] Archivo = reemplazo_espacios.Split(':');

                string[] lista = Archivo[3].Split(',');

            

                for (int i = 0; i <= descomponer.Length - 1; i++)
                {

                    string linea = descomponer[i];

                    string[] dividir = linea.Split(':');

                    string[] temporal = new string[3];

                    if (dividir != null)
                    {

                        Array.Copy(dividir, temporal, Math.Min(dividir.Length, temporal.Length));

                    }

                    dividir = temporal;

                    cuerpo = cuerpo + dividir[0] + ":\t" + dividir[1] + ":\t" + dividir[2] + "\n";


                    if (i == 3)
                    {
                        valorar = temporal[2];
                    }



                }

                try
                {


                    if (valorar == null) // no confirmado el pago
                    {
                        correo.Subject = "NO Confirmado MercadoPago - DUDA - " + DateTime.Now;
                        smtp.Send("correodelosprofesores@gmail.com", "Licenciados@outlook.com.ar", correo.Subject, "MercadoPago no aprovó: " + Nombre_de_archivo + "\n" + cuerpo);

                        StreamWriter web = File.CreateText(Server.MapPath("~/desaprovado/" + Nombre_de_archivo + ".html"));

                        web.Write(reemplazo_comillas);

                        web.Flush();
                        web.Close();
                    }
                    else  // confirmado el pago
                    {
                        correo.Subject = "Confirmacion MercadoPago - Bien - " + DateTime.Now;
                        smtp.Send("correodelosprofesores@gmail.com", "Licenciados@outlook.com.ar", correo.Subject, "MercadoPago está OK: " + Nombre_de_archivo + "\n" + cuerpo);
                
                        StreamWriter web = File.CreateText(Server.MapPath("~/OK/" + Nombre_de_archivo + ".html"));

                        web.Write(reemplazo_comillas);

                        web.Flush();
                        web.Close();
                
                    }
                

                }
                catch (Exception)
                {

                    return;

                }

            }
        }
    }
}