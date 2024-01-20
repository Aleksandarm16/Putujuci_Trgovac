using Google.OrTools.ConstraintSolver;

public class ProblemTrgovackogPutnika
{
    //Napraviti Enum za imena gradova radi lakseg prikaza podataka
    public enum Gradovi
    {
        NewYork = 0,
        LosAngeles = 1,
        Chicago = 2,
        Minneapolis = 3,
        Denver = 4,
        Dallas = 5,
        Seattle = 6,
        Boston = 7,
        SanFrancisco = 8,
        StLouis = 9,
        Houston = 10,
        Phoenix = 11,
        SaltLakeCity = 12
    }
    class DataModel
    {
        //matrica mora biti nxn . Ne moze 13x11, 13x20... 10x10 ili 5x5
        public long[,] MatricaUdaljenosti = {
            { 0, 2451, 713, 1018, 1631, 1374, 2408, 213, 2571, 875, 1420, 2145, 1972 },
            { 2451, 0, 1745, 1524, 831, 1240, 959, 2596, 403, 1589, 1374, 357, 579 },
            { 713, 1745, 0, 355, 920, 803, 1737, 851, 1858, 262, 940, 1453, 1260 },
            { 1018, 1524, 355, 0, 700, 862, 1395, 1123, 1584, 466, 1056, 1280, 987 },
            { 1631, 831, 920, 700, 0, 663, 1021, 1769, 949, 796, 879, 586, 371 },
            { 1374, 1240, 803, 862, 663, 0, 1681, 1551, 1765, 547, 225, 887, 999 },
            { 2408, 959, 1737, 1395, 1021, 1681, 0, 2493, 678, 1724, 1891, 1114, 701 },
            { 213, 2596, 851, 1123, 1769, 1551, 2493, 0, 2699, 1038, 1605, 2300, 2099 },
            { 2571, 403, 1858, 1584, 949, 1765, 678, 2699, 0, 1744, 1645, 653, 600 },
            { 875, 1589, 262, 466, 796, 547, 1724, 1038, 1744, 0, 679, 1272, 1162 },
            { 1420, 1374, 940, 1056, 879, 225, 1891, 1605, 1645, 679, 0, 1017, 1200 },
            { 2145, 357, 1453, 1280, 586, 887, 1114, 2300, 653, 1272, 1017, 0, 504 },
            { 1972, 579, 1260, 987, 371, 999, 701, 2099, 600, 1162, 1200, 504, 0 },
        };
        //Za nas problem dovoljan je jedan trgovacki putnik, ako bi se taj broj povecao onda 
        //je to drugi problem rutiranja vozila i nije u domenu ovog resenja
        public int BrojTrgovaca = 1;
        public int PolazniGrad = 9;
    };

    // in je kljucna rec u C# koja kaze da se vrednost prosledjuje preko reference i da je moguce samo 
    //citati podatke iz nje, a ne menjati podatke
    static void StampajResenje(in RoutingModel ruta, in RoutingIndexManager menadzer, in Assignment resenje)
    {
        Console.WriteLine("Ruta:");
        var index = ruta.Start(0);
        long distancaIzmedjuGradova = 0;
        List<long> distanceGradova = new List<long>();
        var pocetniGrad = ruta.Start(0);
        while (ruta.IsEnd(index) == false)
        {
            Console.Write("{0} "+ (Gradovi)index +" -> ", menadzer.IndexToNode((int)index));
            var prethodniIndex = index;
            index = resenje.Value(ruta.NextVar(index));
            distancaIzmedjuGradova = ruta.GetArcCostForVehicle(prethodniIndex, index, 0);
            distanceGradova.Add(distancaIzmedjuGradova);
        }
        Console.WriteLine("{0} " + (Gradovi)pocetniGrad, menadzer.IndexToNode((int)index));
        Console.WriteLine("Duzina putanje: {0} kilometri", resenje.ObjectiveValue());

        Console.WriteLine("Distance izmedju gradova:");
       for(int i = 0; i < distanceGradova.Count-1;i++)
        {
            Console.Write("{0} -> ", distanceGradova[i]);
        }
        Console.Write("{0}", distanceGradova[distanceGradova.Count-1]);
    }

    public static void Main(String[] args)
    {
        // instanciranje data modela
        DataModel data = new DataModel();

        // Kreiranje indexa menadzera putanje
        RoutingIndexManager menadzer =
            new RoutingIndexManager(data.MatricaUdaljenosti.GetLength(0), data.BrojTrgovaca, data.PolazniGrad);

        // Kreiranje modela rute
        RoutingModel ruta = new RoutingModel(menadzer);

        int indexPovratnogPoziva = ruta.RegisterTransitCallback((long fromIndex, long toIndex) =>
        {
            // Konverzija rute promenjljive u matricu udaljenosti Node
            var fromNode = menadzer.IndexToNode(fromIndex);
            var toNode = menadzer.IndexToNode(toIndex);
            return data.MatricaUdaljenosti[fromNode, toNode];
        });

        // Definisanje cene svakog vozila
        ruta.SetArcCostEvaluatorOfAllVehicles(indexPovratnogPoziva);

        // Postavljanje prvof resenja na defaultnu vrednost.
        RoutingSearchParameters searchParameters =
            operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;

        // Resavanje problema.
        Assignment resenje = ruta.SolveWithParameters(searchParameters);

        // Ispisivanje resenja na konzoli.
        StampajResenje(ruta, menadzer, resenje);
    }
}