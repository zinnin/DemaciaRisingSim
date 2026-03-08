using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.UI;

/// <summary>
/// ViewModel for a single settlement row displayed in the board grid.
/// </summary>
public class SettlementViewModel : INotifyPropertyChanged
{
    private string _structures = string.Empty;
    private int _lumber;
    private int _stone;
    private int _metal;
    private int _petricite;
    private int _food;

    public string SettlementName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;

    public string Structures
    {
        get => _structures;
        set { _structures = value; OnPropertyChanged(); }
    }

    public int Lumber
    {
        get => _lumber;
        set { _lumber = value; OnPropertyChanged(); }
    }

    public int Stone
    {
        get => _stone;
        set { _stone = value; OnPropertyChanged(); }
    }

    public int Metal
    {
        get => _metal;
        set { _metal = value; OnPropertyChanged(); }
    }

    public int Petricite
    {
        get => _petricite;
        set { _petricite = value; OnPropertyChanged(); }
    }

    public int Food
    {
        get => _food;
        set { _food = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Main ViewModel driving the board visualization, simulation controls, and settings.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private string _totalLumber = "0";
    private string _totalStone = "0";
    private string _totalMetal = "0";
    private string _totalPetricite = "0";
    private string _totalFood = "0";
    private string _score = "0";
    private string _statusMessage = "Ready. Click 'Load Default Board' to initialize the board, or 'Optimize' to find the best layout.";
    private bool _isBusy;

    // Settings
    private bool _requireDurandsWorkshop = true;
    private bool _requireShrineOfVeiledLady = true;
    private bool _requireQuartermaster = true;
    private int _maxBuildingLevel = 4;
    private int _foodTargetPerSettlement = 2;

    private Dictionary<string, Settlement> _board;

    public ObservableCollection<SettlementViewModel> Settlements { get; } = [];

    public string TotalLumber
    {
        get => _totalLumber;
        set { _totalLumber = value; OnPropertyChanged(); }
    }

    public string TotalStone
    {
        get => _totalStone;
        set { _totalStone = value; OnPropertyChanged(); }
    }

    public string TotalMetal
    {
        get => _totalMetal;
        set { _totalMetal = value; OnPropertyChanged(); }
    }

    public string TotalPetricite
    {
        get => _totalPetricite;
        set { _totalPetricite = value; OnPropertyChanged(); }
    }

    public string TotalFood
    {
        get => _totalFood;
        set { _totalFood = value; OnPropertyChanged(); }
    }

    public string Score
    {
        get => _score;
        set { _score = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public bool RequireDurandsWorkshop
    {
        get => _requireDurandsWorkshop;
        set { _requireDurandsWorkshop = value; OnPropertyChanged(); }
    }

    public bool RequireShrineOfVeiledLady
    {
        get => _requireShrineOfVeiledLady;
        set { _requireShrineOfVeiledLady = value; OnPropertyChanged(); }
    }

    public bool RequireQuartermaster
    {
        get => _requireQuartermaster;
        set { _requireQuartermaster = value; OnPropertyChanged(); }
    }

    public int MaxBuildingLevel
    {
        get => _maxBuildingLevel;
        set { _maxBuildingLevel = value; OnPropertyChanged(); }
    }

    public int FoodTargetPerSettlement
    {
        get => _foodTargetPerSettlement;
        set { _foodTargetPerSettlement = Math.Max(0, value); OnPropertyChanged(); }
    }

    public MainViewModel()
    {
        _board = BoardData.CreateDefaultBoard();
        LoadBoard(_board);
    }

    /// <summary>Loads and displays the default board without optimization.</summary>
    public void LoadDefault()
    {
        _board = BoardData.CreateDefaultBoard();
        LoadBoard(_board);
        StatusMessage = "Default board loaded (all Lumberyard level 1).";
    }

    /// <summary>Runs optimization on the current board and updates the display.</summary>
    public async Task OptimizeAsync()
    {
        IsBusy = true;
        StatusMessage = "Optimizing board layout…";
        try
        {
            var settings = new SimulationSettings
            {
                RequireDurandsWorkshop  = RequireDurandsWorkshop,
                RequireShrineOfVeiledLady = RequireShrineOfVeiledLady,
                RequireQuartermaster    = RequireQuartermaster,
                MaxBuildingLevel        = MaxBuildingLevel,
                FoodTargetPerSettlement = FoodTargetPerSettlement,
            };
            var boardToOptimize = BoardData.Clone(_board);
            var optimized = await Task.Run(() => Simulator.OptimizeBoard(boardToOptimize, settings));
            _board = optimized;
            LoadBoard(_board);
            StatusMessage = $"Optimization complete. Score: {Simulator.Score(_board):F4}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadBoard(Dictionary<string, Settlement> board)
    {
        Settlements.Clear();

        foreach (var kv in board)
        {
            var settlement = kv.Value;
            var output = Simulator.SettlementOutput(settlement);

            Settlements.Add(new SettlementViewModel
            {
                SettlementName = settlement.Name,
                Environment    = settlement.Environment.ToString(),
                Structures     = string.Join(", ", settlement.Structures.Select(s => s.ToString())),
                Lumber         = output.Lumber,
                Stone          = output.Stone,
                Metal          = output.Metal,
                Petricite      = output.Petricite,
                Food           = output.Food,
            });
        }

        UpdateTotals(board);
    }

    private void UpdateTotals(Dictionary<string, Settlement> board)
    {
        var total = Simulator.BoardOutput(board);
        TotalLumber    = total.Lumber.ToString();
        TotalStone     = total.Stone.ToString();
        TotalMetal     = total.Metal.ToString();
        TotalPetricite = total.Petricite.ToString();
        TotalFood      = total.Food.ToString();
        Score          = Simulator.Score(total).ToString("F4");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
