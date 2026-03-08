using System.Diagnostics;
using DemaciaRisingSim.Core;

// ── Argument parsing ─────────────────────────────────────────────────────────
var settings = new SimulationSettings();
bool verbose = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i].ToLowerInvariant())
    {
        case "--no-workshop":  settings.RequireDurandsWorkshop    = false; break;
        case "--no-shrine":    settings.RequireShrineOfVeiledLady = false; break;
        case "--no-qm":        settings.RequireQuartermaster       = false; break;
        case "--verbose":      verbose = true;                              break;
        case "--max-level"  when i + 1 < args.Length:
            settings.MaxBuildingLevel        = int.Parse(args[++i]); break;
        case "--food"       when i + 1 < args.Length:
            settings.FoodTargetPerSettlement = int.Parse(args[++i]); break;
        case "--lumber"     when i + 1 < args.Length:
            settings.LumberTarget            = int.Parse(args[++i]); break;
        case "--stone"      when i + 1 < args.Length:
            settings.StoneTarget             = int.Parse(args[++i]); break;
        case "--metal"      when i + 1 < args.Length:
            settings.MetalTarget             = int.Parse(args[++i]); break;
        case "--petricite"  when i + 1 < args.Length:
            settings.PetriciteTarget         = int.Parse(args[++i]); break;
        case "--help":
            PrintHelp();
            return 0;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}  (run with --help for usage)");
            return 1;
    }
}

// ── Banner ───────────────────────────────────────────────────────────────────
Console.OutputEncoding = System.Text.Encoding.UTF8;
const string Title = "Demacia Rising Simulator — Console Runner";
Console.WriteLine(Title);
Console.WriteLine(new string('═', Title.Length));
Console.WriteLine();

// ── Settings summary ─────────────────────────────────────────────────────────
Console.WriteLine("Settings");
Console.WriteLine($"  Max building level    : {settings.MaxBuildingLevel}");
Console.WriteLine($"  Food target/settle.   : {settings.FoodTargetPerSettlement}");
Console.WriteLine($"  Durand's Workshop     : {(settings.RequireDurandsWorkshop    ? "required" : "skipped")}");
Console.WriteLine($"  Shrine of Veiled Lady : {(settings.RequireShrineOfVeiledLady  ? "required" : "skipped")}");
Console.WriteLine($"  Quartermaster         : {(settings.RequireQuartermaster        ? "required" : "skipped")}");
Console.WriteLine($"  Lumber target         : {settings.LumberTarget,9:N0}");
Console.WriteLine($"  Stone  target         : {settings.StoneTarget,9:N0}");
Console.WriteLine($"  Metal  target         : {settings.MetalTarget,9:N0}");
Console.WriteLine($"  Petricite target      : {settings.PetriciteTarget,9:N0}");
Console.WriteLine();

// ── Optimise ─────────────────────────────────────────────────────────────────
Console.WriteLine("Optimizing…");

Action<string>? progress = verbose ? msg => Console.WriteLine($"  {msg}") : null;

var sw       = Stopwatch.StartNew();
var board    = BoardData.CreateDefaultBoard();
var optimized = Simulator.OptimizeBoard(board, settings, progress);
sw.Stop();

Console.WriteLine($"Done in {sw.Elapsed.TotalSeconds:F2} s.");
Console.WriteLine();

// ── Result ───────────────────────────────────────────────────────────────────
Console.Write(Simulator.FullReport(optimized, settings));
return 0;

// ── Help ─────────────────────────────────────────────────────────────────────
static void PrintHelp()
{
    Console.WriteLine("Usage: DemaciaRisingSim.Console [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --max-level N       Maximum building level to try (1–4, default 4)");
    Console.WriteLine("  --food N            Minimum food per settlement (default 2)");
    Console.WriteLine("  --no-workshop       Skip placing Durand's Workshop");
    Console.WriteLine("  --no-shrine         Skip placing the Shrine of the Veiled Lady");
    Console.WriteLine("  --no-qm             Skip placing the Quartermaster");
    Console.WriteLine("  --lumber N          Lumber target  (default 296,300)");
    Console.WriteLine("  --stone N           Stone  target  (default 343,400)");
    Console.WriteLine("  --metal N           Metal  target  (default 143,650)");
    Console.WriteLine("  --petricite N       Petricite target (default 1,450)");
    Console.WriteLine("  --verbose           Print per-iteration progress during optimization");
    Console.WriteLine("  --help              Show this help");
}
