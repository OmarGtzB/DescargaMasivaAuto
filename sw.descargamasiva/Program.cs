using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;


namespace sw.descargamasiva
{
    public class Program
    {
        static string rutaPfx = @"C:\Archivo\pfx.pfx";// = @"\\192.168.1.200\Intelisis\XML\Resources\pfx.pfx";
        static string rutaConfig = @"C:\Archivo\Configuracion.txt";
        
        //static byte[] pfx = File.ReadAllBytes(@"Resources\pfx.pfx");//\\192.168.1.200\Intelisis\XML\Resources
        static byte[] pfx;//\\192.168.1.200\Intelisis\XML\Resources
        static string password;

        static string urlAutentica = "https://cfdidescargamasivasolicitud.clouda.sat.gob.mx/Autenticacion/Autenticacion.svc";
        static string urlAutenticaAction = "http://DescargaMasivaTerceros.gob.mx/IAutenticacion/Autentica";

        static string urlSolicitud = "https://cfdidescargamasivasolicitud.clouda.sat.gob.mx/SolicitaDescargaService.svc";
        static string urlSolicitudAction = "http://DescargaMasivaTerceros.sat.gob.mx/ISolicitaDescargaService/SolicitaDescarga";

        static string urlVerificarSolicitud = "https://cfdidescargamasivasolicitud.clouda.sat.gob.mx/VerificaSolicitudDescargaService.svc";
        static string urlVerificarSolicitudAction = "http://DescargaMasivaTerceros.sat.gob.mx/IVerificaSolicitudDescargaService/VerificaSolicitudDescarga";

        static string urlDescargarSolicitud = "https://cfdidescargamasiva.clouda.sat.gob.mx/DescargaMasivaService.svc";
        static string urlDescargarSolicitudAction = "http://DescargaMasivaTerceros.sat.gob.mx/IDescargaMasivaTercerosService/Descargar";

        static string RfcEmisor;
        static string RfcReceptor;
        static string RfcSolicitante;
        static string FechaInicial;
        static string FechaFinal;
        static string Dato;
        static string PathXmlRecibido;
        static string PathXmlEmitido;
        static string Bandera;
        static string TipoSolicitud;

        static string valUuid;
        static string valDocXml;
        static string pathFinal = @"C:\Users\programador\Desktop\xmlMover";
        //\\192.168.1.200\Intelisis\CE\ADD\No Validados\Egresos\Compras

        static string rutaPrueba = @"C:\Users\programador\Desktop\xmlPrueba";
        static void Main(string[] args)
        {
            //for (int i = 1; i <= 4; i++)
            //{
            //    ObtieneParametros(4);
            //    Accion();
            //    System.Threading.Thread.Sleep(20000);
            //}

            InsertaXml(rutaPrueba);


        }

        public static void ObtieneParametros( int valor)
        {
            char separador = ',';
            string registro = "", rfc = "", pathDescargaEmitido = "", pathDescargaRecibido = "";
            string x = "";
            string diasEmitidos = "", diasRecibidos="";
            string dato = "";
            Bandera = "";

            if (!(File.Exists(rutaConfig)))
            {
                Console.WriteLine("El archivo de configuracion no se encuentra en la Ruta C:\\Archivo\\Configuracion.txt");
            }
            else { 
                try
            {
               
               //Obtiene la informacion del archivo de configuracion

                //System.IO.StreamReader file = new System.IO.StreamReader(@"\\192.168.1.200\Intelisis\ArchivoConfig\Configuracion.txt");
                System.IO.StreamReader file = new System.IO.StreamReader(rutaConfig);
                x = file.ReadToEnd();
                string[] vector = x.Split(new char[] { '\n' });

                registro = vector[0].Replace("\r", "");
                rfc = vector[1].Replace("\r", "");
                diasEmitidos = vector[2].Replace("\r", "");
                diasRecibidos = vector[3].Replace("\r", "");
                dato = vector[4].Replace("\r", "");
                pathDescargaEmitido = vector[5].Replace("\r", "");
                pathDescargaRecibido = vector[6].Replace("\r", "");

                file.Close();

                //obtiene el valor del registro
                string[] arreglo = registro.Split(separador);
                String valorRegistro = arreglo[1].Replace(" ", String.Empty);

                //obtiene el valor del rfc
                string[] arreglo2 = rfc.Split(separador);
                String valorRfc = arreglo2[1].Replace(" ", String.Empty);
                

                //obtiene el valor de los dias de inicio para Emitidos/Recibidos
                string[] arreglo3 = diasEmitidos.Split(separador);
                int DiasEmite = Convert.ToInt32(arreglo3[1]);

                string[] aregloRecibidos = diasRecibidos.Split(separador);
                int DiasRecibe = Convert.ToInt32(aregloRecibidos[1]);


                //obtiene el valor de la semilla
                string[] arreglo4 = dato.Split(separador);
                Dato = arreglo4[1].Replace(" ", String.Empty);          

                //obtiene el valor de la ruta de la descarga xml Emitidos/Recibidos
                string[] arreglo5 = pathDescargaEmitido.Split(separador);
                pathDescargaEmitido = arreglo5[1].Replace(" ", String.Empty);

                string[] arreglo6 = pathDescargaRecibido.Split(separador);
                pathDescargaRecibido = arreglo6[1].Replace(" ", String.Empty);

                var dtE = DateTime.Today.AddDays(-DiasEmite);
                var dtR = DateTime.Today.AddDays(-DiasRecibe);
                var dtFin = DateTime.Today.AddDays(-1);

                //valida la fecha si es el ultimo dia del mes obtiene la descarga de 
                //los dos ultimos meses
                var Fecha_FinMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1).ToString("dd-MM-yyyy");
                var FechaSistema =  DateTime.Now.ToString("dd-MM-yyyy"); 

                if (valor == 1 || valor == 2)
                {
                    if (FechaSistema == Fecha_FinMes)
                    {
                        //asigna fecha inicial 2 meses atras
                        var Fecha_InicialMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
                        dtE = Fecha_InicialMes;
                        dtR = Fecha_InicialMes;
                        dtFin = Convert.ToDateTime(Fecha_FinMes);
                    }
                }

                if (valor == 3 || valor == 4)
                {
                    if (FechaSistema == Fecha_FinMes)
                    {
                        //asigna fecha inicial 2 meses atras
                        var Fecha_InicialMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-2);
                        dtE = Fecha_InicialMes;
                        dtR = Fecha_InicialMes;
                        dtFin = Convert.ToDateTime(Fecha_FinMes);
                    }
                }



                //valor 1 descarga xml recibidos / valor 2 Emitidos
                if (valor == 1)
                {
                       

                    RfcEmisor = "";
                    RfcReceptor = valorRfc;
                    RfcSolicitante = RfcReceptor;
                    Bandera = "Recibidos";
                    FechaInicial = String.Format("{0:yyyy-MM-dd}", dtR);
                    PathXmlRecibido = pathDescargaRecibido;
                    TipoSolicitud = "CFDI";

                }
                else if (valor == 2)
                {
                    RfcEmisor = valorRfc;
                    RfcSolicitante = RfcEmisor;
                    RfcReceptor = "";
                    Bandera = "Emitidos";
                    FechaInicial = String.Format("{0:yyyy-MM-dd}", dtE);
                    PathXmlEmitido = pathDescargaEmitido;
                    TipoSolicitud = "CFDI";
                }
                else if (valor == 3)
                {
                    RfcEmisor = "";
                    RfcReceptor = valorRfc;
                    RfcSolicitante = RfcReceptor;
                    Bandera = "MetadataRecibidos";
                    FechaInicial = String.Format("{0:yyyy-MM-dd}", dtR);
                    PathXmlRecibido = pathDescargaRecibido;
                    TipoSolicitud = "Metadata";
                }
                else if (valor == 4)
                {
                    RfcEmisor = valorRfc;
                    RfcSolicitante = RfcEmisor;
                    RfcReceptor = "";
                    Bandera = "MetadataEmitidos";
                    FechaInicial = String.Format("{0:yyyy-MM-dd}", dtE);
                    PathXmlEmitido = pathDescargaEmitido;
                    TipoSolicitud = "Metadata";
                }
                password = valorRegistro;
                FechaFinal = String.Format("{0:yyyy-MM-dd}", dtFin);
               
                


                    //FechaInicial = String.Format("{0:yyyy-MM-dd}", "2020-01-01");
                    //FechaFinal = String.Format("{0:yyyy-MM-dd}", "2020-12-31");
                    //RfcReceptor = "GACJ8504306G7";            





                }
            catch (FileNotFoundException fe)
            {
               Console.WriteLine("Error: "+ fe.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
          
            }

            }
        }
        public static void Accion()
        {

            if (!(File.Exists(rutaPfx)))
            {
                Console.WriteLine(@"El archivo pfx no se encuentra en la Ruta C:\Archivo\pfx.pfx");
            }
            else
            {
                pfx = File.ReadAllBytes(rutaPfx);
                //Obtener Certificados
                X509Certificate2 certifcate = ObtenerX509Certificado(pfx, Dato);
                //X509Certificate2 certifcate = ObtenerX509Certificado(pfx);

                //Obtener Token
                string token = ObtenerToken(certifcate);
                string autorization = String.Format("WRAP access_token=\"{0}\"", HttpUtility.UrlDecode(token));
                Console.WriteLine("Token: " + token);

                //Generar Solicitud
                string idSolicitud = GenerarSolicitud(certifcate, autorization);
                Console.WriteLine("IdSolicitud: " + idSolicitud);

                if (!string.IsNullOrEmpty(idSolicitud))
                {
                    //Validar Solicitud
                    string idPaquete = ValidarSolicitud(certifcate, autorization, idSolicitud);
                    Console.WriteLine("IdPaquete: " + idPaquete);

                    if (string.IsNullOrEmpty(idPaquete))
                    {

                        for (int i = 1; i <= 10; i++)
                        {
                            System.Threading.Thread.Sleep(180000);
                            //Validar Solicitud
                            idPaquete = ValidarSolicitud(certifcate, autorization, idSolicitud);
                            Console.WriteLine("IdPaquete: " + idPaquete);

                            if (!string.IsNullOrEmpty(idPaquete))
                            {
                                i = 10;
                                // Descargar Solicitud
                                string descargaResponse = DescargarSolicitud(certifcate, autorization, idPaquete);
                                GuardarSolicitud(idPaquete, descargaResponse);
                            }
                        }
                        if (string.IsNullOrEmpty(idPaquete))
                        {
                            Console.WriteLine("Fin del proceso");

                            Environment.Exit(1);
                        }


                    }
                    else
                    {
                        //Descargar Solicitud
                        string descargaResponse = DescargarSolicitud(certifcate, autorization, idPaquete);
                        GuardarSolicitud(idPaquete, descargaResponse);
                    }

                }
                else
                {
                    Console.WriteLine("No se genero IdSolicitud  ");

                   
                } 


               

            }



        }
        //private static X509Certificate2 ObtenerX509Certificado(byte[] pfx)
        private static X509Certificate2 ObtenerX509Certificado(byte[] pfx, string semi)
        {
            Encriptar enc = new Encriptar();
       
            return new X509Certificate2(pfx, enc.Decrypt(password, semi), X509KeyStorageFlags.MachineKeySet |
            //return new X509Certificate2(pfx, password, X509KeyStorageFlags.MachineKeySet |
                            X509KeyStorageFlags.PersistKeySet |
                            X509KeyStorageFlags.Exportable);
        }
        
        private static void GuardarSolicitud(string idPaquete, string descargaResponse)
        {
            try
            {
                string PathXml = String.Empty;
              
                if(Bandera == "Emitidos")
                {

                    if (string.IsNullOrEmpty(PathXmlEmitido))
                    {
                        PathXmlEmitido = @"C:\Archivo\xml";
                        if (!(Directory.Exists(PathXmlEmitido)))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(PathXmlEmitido);
                        }
                       
                        PathXml = PathXmlEmitido;
                    }
                    else
                    {
                        PathXml = PathXmlEmitido;
                    }
                    
                }
                if (Bandera == "Recibidos")
                {
                    if(string.IsNullOrEmpty(PathXmlRecibido))
                    {
                        PathXmlRecibido = @"C:\Archivo\xml";                        
                        if (!(Directory.Exists(PathXmlRecibido)))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(PathXmlRecibido);
                        }

                        PathXml = PathXmlRecibido;
                    }
                    else
                    {
                        PathXml = PathXmlRecibido;
                    }
                }
                if (Bandera == "MetadataEmitidos")
                {
                    if (string.IsNullOrEmpty(PathXmlEmitido))
                    {
                        PathXmlEmitido = @"C:\Archivo\xml\Metadata";
                        if (!(Directory.Exists(PathXmlEmitido)))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(PathXmlEmitido);
                        }

                        PathXml = PathXmlEmitido;
                    }
                    else
                    {
                        PathXml = PathXmlEmitido;
                    }
                }
                if (Bandera == "MetadataRecibidos")
                {
                    if (string.IsNullOrEmpty(PathXmlRecibido))
                    {
                        PathXmlRecibido = @"C:\Archivo\xml\Metadata";
                        if (!(Directory.Exists(PathXmlRecibido)))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(PathXmlRecibido);
                        }

                        PathXml = PathXmlRecibido;
                    }
                    else
                    {
                        PathXml = PathXmlRecibido;
                    }

                }

                if (!(Directory.Exists(PathXml)))
                {
                    Directory.CreateDirectory(PathXml);
                }

                byte[] file = Convert.FromBase64String(descargaResponse);
                Directory.CreateDirectory(PathXml);

                using (FileStream fs = File.Create(PathXml + idPaquete + ".zip", file.Length))
                {
                    fs.Write(file, 0, file.Length);
                }
                Console.WriteLine("FileCreated: " + PathXml + idPaquete + ".zip");

                Descomprimir(PathXml + idPaquete + ".zip", PathXml);
                try
                {
                    //inserta xml en tabla xmlDoc
                    if (Bandera == "Recibidos" || Bandera == "Emitidos")
                    {
                        //metodo para insertar datos a la tabla xmlDoc
                        InsertaXml(PathXml);
                    }
                   
                    if (Bandera == "MetadataRecibidos" || Bandera == "MetadataEmitidos")
                    {
                        //Inserta la metadata a la bd 
                        string rutaMetadata = "";
                        rutaMetadata = PathXml + idPaquete + ".txt";
                        InstertaMetadata(rutaMetadata);
                    }

                }catch(Exception ee)
                {
                    Console.WriteLine("General Error: " + ee.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("General Error: " + e.Message);


            }


        }

        private static string DescargarSolicitud(X509Certificate2 certifcate, string autorization, string idPaquete)
        {
            DescargarSolicitud descargarSolicitud = new DescargarSolicitud(urlDescargarSolicitud, urlDescargarSolicitudAction);
            string xmlDescarga = descargarSolicitud.Generate(certifcate, RfcSolicitante, idPaquete);
            return descargarSolicitud.Send(autorization);
        }

        private static string ValidarSolicitud(X509Certificate2 certifcate, string autorization, string idSolicitud)
        {
            VerificaSolicitud verifica = new VerificaSolicitud(urlVerificarSolicitud, urlVerificarSolicitudAction);
            string xmlVerifica = verifica.Generate(certifcate, RfcSolicitante, idSolicitud);
            System.Threading.Thread.Sleep(40000);
            return verifica.Send(autorization);
        }

        private static string GenerarSolicitud(X509Certificate2 certifcate, string autorization)
        {
            Solicitud solicitud = new Solicitud(urlSolicitud, urlSolicitudAction);
            string xmlSolicitud = solicitud.Generate(certifcate, RfcEmisor, RfcReceptor, RfcSolicitante, FechaInicial, FechaFinal,TipoSolicitud);
            return solicitud.Send(autorization);
        }

        private static string ObtenerToken(X509Certificate2 certifcate)
        {
            Autenticacion service = new Autenticacion(urlAutentica, urlAutenticaAction);
            string xml = service.Generate(certifcate);
            return service.Send();
        }

        public static void Descomprimir(string rutaZip,string rutaDestino)
        {
            //string pathDestino = @"\\192.168.1.200\Intelisis\XML\Paquetes\";

            try
            {
                if (File.Exists(rutaZip))
                {
                    
                    ZipFile.ExtractToDirectory(rutaZip, rutaDestino);
                    Console.WriteLine("Extraccion de archivos correctamente!!!");
                    File.Delete(rutaZip);
                }
                else
                {
                    Console.WriteLine("El archivo No existe...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: "+ ex);
            }
           
        }

       public static void InstertaMetadata(string rutaArchivo)
        {
            try
            {
                //Inserta datos a la BD
                ConexionBD oCone = new ConexionBD();

                oCone.ExecuteSP1("spCFDIDescargaMetadata",
                              new SqlParameter("@ruta", rutaArchivo));
                              //new SqlParameter("@Param2", /* Tu valor */),
                              //new SqlParameter("@Param3", /* Tu valor */),
                              //new SqlParameter("@Param4", /* Tu valor */));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Security error.\n\nError message: {e.Message}\n\n" +
                   $"Details:\n\n{e.StackTrace}");
            }
        }

        public static void InsertaXml(string rutaCarpeta)
        {
            int contador = 0;
            try
            {

                DirectoryInfo di = new DirectoryInfo(@rutaCarpeta);

                //For que va recorriendo los archivos obtenidos de la carpeta seleccionada
                foreach (var item in di.GetFiles())
                {
                    string nomArchivo = "";
                    string rutaArchivo = "";
                    string extension = "";


                    //obtiene la extencion del archivo
                    extension = Path.GetExtension(item.Name);
                    //Valida los archivos que solo sean  XML
                    if (extension == ".xml")
                    {
                  
                        rutaArchivo = @rutaCarpeta + @"\" + (nomArchivo = item.Name);
                        //agregar funcionabilidad de extraccion de uuid
                        ExtraccionUuid(rutaArchivo, nomArchivo);


                        contador = contador + 1;
                    }
                    else
                    {
                        Console.WriteLine("Solo se permiten archivos XML");
                        //break;
                    }


                }


            }
            catch (Exception e)
            {
                Console.WriteLine($"Security error.\n\nError message: {e.Message}\n\n" +
                   $"Details:\n\n{e.StackTrace}");
            }
        }

        public static void ExtraccionUuid(string RutaArchi, string NombreArch)
        {
            //Metodo para obtener el xml y guardarlo en la variable valDocXml
            UsingXMLReader(RutaArchi);
            try
            {
                //Inserta datos a la BD
                ConexionBD oDatosRecupera = new ConexionBD();
                oDatosRecupera.InsertaTabla(valDocXml, valUuid);

                //ValidaDirectorioFinal();


                //Mueve el archivo 
                if ((valDocXml != null) || (valDocXml != ""))
                {

                    string archivoFinal = pathFinal + NombreArch;

                    if (Directory.Exists(pathFinal))
                    {
                        MoverArchivo(RutaArchi, archivoFinal);
                    }
                    else
                    {
                        Directory.CreateDirectory(pathFinal);
                        MoverArchivo(RutaArchi, archivoFinal);
                    }

                }
                //MessageBox.Show("Uuid : " + valUuid);
                //LlenaGrid();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Security error.\n\nError message: {e.Message}\n\n" +
                    $"Details:\n\n{e.StackTrace}");
            }
        }
        public static void UsingXMLReader(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList xTimbreFiscalDigital = doc.GetElementsByTagName("tfd:TimbreFiscalDigital");
                valDocXml = doc.OuterXml;


               
            }
            catch (Exception e)
            {
                Console.WriteLine("Error :" + e);
            }
        }

        public static void MoverArchivo(string rutaIni, string rutaFin)
        {

            try
            {
                if (File.Exists(rutaFin))
                {

                }
                else
                {
                    //File.Copy(rutaIni, rutaFin);
                    //File.Move(rutaIni, rutaFin); 
                    System.IO.File.Move(rutaIni, rutaFin);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
            }
        }

    }
}
