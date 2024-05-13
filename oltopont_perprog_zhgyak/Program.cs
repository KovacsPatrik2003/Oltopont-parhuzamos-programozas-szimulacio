using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

namespace oltopont_perprog_zhgyak
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Enumerable.Range(1, 3).Select(x => new Orvos()).ToList().Select(x => new Task(() => x.Work(), TaskCreationOptions.LongRunning)).ToList().ForEach(x => x.Start());
            Enumerable.Range(1, 10).Select(x => new Paciens()).ToList().Select(x => new Task(() => x.Work(), TaskCreationOptions.LongRunning)).ToList().ForEach(x => x.Start());
            Paciens.SorGenerallas();
            new Task(() =>
            {
                while (Paciens.osszesPaciens.Any(x=>x.Status!=PaciensStatus.Hazamnet))
                {
                    Console.SetCursorPosition(0, 0);
                    foreach (var item in Orvos.osszesOrvos)
                    {
                        Console.WriteLine($"Orvos: {item.id} {item.Status.PadRight(Console.WindowWidth - 30, ' ')}");
                    }
                    Console.WriteLine();
                    foreach (var item in Paciens.osszesPaciens)
                    {
                        Console.WriteLine($"Paciens: {item.id} Idopontja: {item.Idopont} OrvosID: {item.OrovosID}  {item.Status.ToString().PadRight(Console.WindowWidth - 60, ' ')}");
                    }
                    Thread.Sleep(100);
                }
                Console.SetCursorPosition(0, 0);
                foreach (var item in Orvos.osszesOrvos)
                {
                    Console.WriteLine($"Orvos: {item.id} {item.Status.PadRight(Console.WindowWidth - 30, ' ')}");
                }
                Console.WriteLine();
                foreach (var item in Paciens.osszesPaciens)
                {
                    Console.WriteLine($"Paciens: {item.id} Idopontja: {item.Idopont} OrvosID: {item.OrovosID}  {item.Status.ToString().PadRight(Console.WindowWidth - 60, ' ')}");
                }
            }).Start();
            Console.ReadLine();
        }
    }
    public static class Util
    {
        public static Random rnd = new Random();
    }
    public enum PaciensStatus { Varakozas, SorbanAllas, Adminisztarcio,Oltas, Varoban,Hazamnet}
    public class Paciens
    {
        public int id { get; set; }
        static int _id=1;
        public static int osszesPaciensCount=0;
        public static List<Paciens> sorbanAllas = new List<Paciens>();
        public static List<Paciens> osszesPaciens = new List<Paciens>();
        public string Idopont { get; set; }
        public static object lockObject = new object();
        public PaciensStatus Status { get; set; }
        public int OrovosID { get; set; }
        public Paciens()
        {
            id = _id++;
            osszesPaciens.Add(this);
            Status = PaciensStatus.Varakozas;
            
            
        }
        public static void SorGenerallas()
        {
            int oraszamlalo = 9;
            int percszamlalo = 0;
            for (int i = 0; i < osszesPaciens.Count; i++)
            {
                if (percszamlalo<60)
                {
                    osszesPaciens[i].Idopont = $"{oraszamlalo} ora {percszamlalo} perc";
                    percszamlalo += 5;
                }
                else
                {
                    percszamlalo = 0;
                    oraszamlalo++;
                    osszesPaciens[i].Idopont = $"{oraszamlalo} ora {percszamlalo} perc";
                }
                
            }
            
        }
        public void Work()
        {
            OrovosID = -1;
            lock (lockObject)
            {
                Status = PaciensStatus.Adminisztarcio;
                Thread.Sleep(Util.rnd.Next(1000, 3001));
            }
            Orvos o = Orvos.osszesOrvos.OrderBy(x => Util.rnd.Next(1, 101)).Select(x => x).FirstOrDefault();

            o.SajatSor.Enqueue(this);
            OrovosID = o.id;
            Status = PaciensStatus.SorbanAllas;
            lock (o.lockObject)
            {
                while (!o.Status.Contains($"Dolgozik   Paciens id: {this.id}") && o.Status.Contains("Dolgozik"))
                {
                    Monitor.Wait(o.lockObject);
                    
                }
                Status = PaciensStatus.Oltas;
                Thread.Sleep(Util.rnd.Next(3000, 7000));

            }
            Status = PaciensStatus.Varoban;
            Thread.Sleep(Util.rnd.Next(15000, 30001));
            Status = PaciensStatus.Hazamnet;
        }
    }
    public class Orvos
    {
        public int id { get; set; }
        static int _id = 1;
        public static List<Orvos> osszesOrvos=new List<Orvos>();
        public object lockObject =new object();
        public string Status { get; set; }
        public Queue<Paciens> SajatSor { get; set; }
        int oltasSzama=0;
        public Orvos()
        {
            id= _id++;
            osszesOrvos.Add(this);
            SajatSor = new Queue<Paciens>();
            Status = "Varakozik";
        }
        public void Work()
        {
            while (Paciens.osszesPaciens.Any(x=>x.Status!=PaciensStatus.Varoban))
            {


                Paciens p;
                try
                {
                    p = SajatSor.Dequeue();
                }
                catch (Exception)
                {
                    //Status = "Várakozik";
                    //Monitor.Pulse(lockObject);
                    continue;
                }
                lock (lockObject)
                {
                    
                    Status = $"Dolgozik   Paciens id: {p.id}";
                    oltasSzama++;
                    if (oltasSzama==8)
                    {
                        Status = "Dolgozik a vancinak cserejen";
                        Thread.Sleep(Util.rnd.Next(1000, 3001));
                        oltasSzama = 0;
                    }
                    else
                    {
                        int esely = Util.rnd.Next(5, 8);
                        if (esely==oltasSzama)
                        {
                            Status = "Dolgozik a vancinak cserejen";
                            Thread.Sleep(Util.rnd.Next(1000, 3001));
                            oltasSzama = 0;
                        }
                    }
                    Monitor.Pulse(lockObject);
                    



                }
                //Status = "Varakozik";

            }
            Status = "Vegzett";
        }
    }
}
