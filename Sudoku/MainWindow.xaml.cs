using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Sudoku {
    public partial class MainWindow : Window {
        // Determines if the some tiles will be removed or all will contain numbers
        bool debug = true;

        // Utilities
        private Random rnd = new Random();
        private System.Timers.Timer timer = new System.Timers.Timer();
        private double generationTime;
        private float time;

        // Game Related
        private bool gm_easy = true, gm_normal, gm_hard, usingTimer, usingUndo;
        private int currentNumber = 1, Undos = 0, x = 0, currentLevel = 1;
        private static Dictionary<string, int> tiles;
        private List<string> boards, solvedBoards;
        private string currentTime = "";
        

        public MainWindow()
        {
            InitializeComponent();

            // Basically a Start Function
            if (x == 0) {
                x = 1;

                // [Letter] = 3x3 Group letter, [1st Number] = Horizontal Group, [2nd Number] = Vertical Group
                tiles = new Dictionary<string, int>(){

                    {"a11", 0 },{"a12", 0 },{"a13", 0 },  {"b14", 0 },{"b15", 0 },{"b16", 0 },  {"c17", 0 },{"c18", 0 },{"c19", 0 },
                    {"a21", 0 },{"a22", 0 },{"a23", 0 },  {"b24", 0 },{"b25", 0 },{"b26", 0 },  {"c27", 0 },{"c28", 0 },{"c29", 0 },
                    {"a31", 0 },{"a32", 0 },{"a33", 0 },  {"b34", 0 },{"b35", 0 },{"b36", 0 },  {"c37", 0 },{"c38", 0 },{"c39", 0 },

                    {"d41", 0 },{"d42", 0 },{"d43", 0 },  {"e44", 0 },{"e45", 0 },{"e46", 0 },  {"f47", 0 },{"f48", 0 },{"f49", 0 },
                    {"d51", 0 },{"d52", 0 },{"d53", 0 },  {"e54", 0 },{"e55", 0 },{"e56", 0 },  {"f57", 0 },{"f58", 0 },{"f59", 0 },
                    {"d61", 0 },{"d62", 0 },{"d63", 0 },  {"e64", 0 },{"e65", 0 },{"e66", 0 },  {"f67", 0 },{"f68", 0 },{"f69", 0 },

                    {"g71", 0 },{"g72", 0 },{"g73", 0 },  {"h74", 0 },{"h75", 0 },{"h76", 0 },  {"i77", 0 },{"i78", 0 },{"i79", 0 },
                    {"g81", 0 },{"g82", 0 },{"g83", 0 },  {"h84", 0 },{"h85", 0 },{"h86", 0 },  {"i87", 0 },{"i88", 0 },{"i89", 0 },
                    {"g91", 0 },{"g92", 0 },{"g93", 0 },  {"h94", 0 },{"h95", 0 },{"h96", 0 },  {"i97", 0 },{"i98", 0 },{"i99", 0 }

                }; // Tiles

                // Pre-Made Boards [Unsolved] 
                boards = new List<string>(){
                    "", // Random Board
                    "000260701680070090190004500820100040004602900050003028009300074040050036703018000",
                    "100489006730000040000001295007120600500703008006095700914600000020000037800512004",
                    "020608000580009700000040000370000500600000004008000013000020000009800036000306090",
                    "916004072800620050500008930060000200000207000005000090097800003080076009450100687",
                    "000300070009500042070400980000002007003105600100900000068003050530001700010008000",
                    "002800010074301080000024000600500900000080000008002005000730000080406720040008300",
                    "060001907100007230080000406018002004070040090900100780607000040051600009809300020",
                    "000000860000807000800036102700000093005000400180000006608190007000203000034000000",
                    "300060250000500103005210486000380500030000040002045000413052700807004000056070004"
                };

                // Pre-Made Boards [Solved]
                solvedBoards = new List<string>(){
                    "", // Random Board
                    "435269781682571493197834562826195347374682915951743628519326874248957136763418259",
                    "152489376739256841468371295387124659591763428246895713914637582625948137873512964",
                    "123678945584239761967145328372461589691583274458792613836924157219857436745316892",
                    "916354872873629154524718936768935241149287365235461798697842513381576429452193687",
                    "245389176689517342371426985496832517823175694157964823968743251534291768712658439",
                    "562879413974351682813624597621547938759183264438962175296735841385416729147298356",
                    "463281957195467238782539416518792364276843195934156782627918543351624879849375621",
                    "473921865261857349859436172746518293395672418182349756628194537517263984934785621",
                    "381467259624598173975213486749381562538726941162945837413652798897134625256879314"
                };

            }
        }

        // Scene / Window Functions
        public void open_scene(System.Windows.UIElement current, System.Windows.UIElement new_)
        {
            // Exit Scene
            current.Opacity = 0;
            current.IsEnabled = false;
            Panel.SetZIndex(current, 0);

            // Enter Scene
            new_.Opacity = 100;
            new_.IsEnabled = true;
            Panel.SetZIndex(new_, 1);
        }
        public void exit_window() { this.Close(); }

        // Board Generation
        public string SeedGenerator(List<KeyValuePair<string, int>> tile) {

            string seed = "";

            // Random Numbers 1-9 in Each Group
            string n = "123456789", group = "abcdefghi";
            for (int i = 0; i < 9; i++)
            {
                List<string> numbers = new List<string>();
                foreach (char c in n) { numbers.Add(c.ToString()); }

                foreach (KeyValuePair<string, int> kv in tile)
                {
                    if (kv.Key[0].ToString() == group[i].ToString())
                    {
                        int random = rnd.Next(0, numbers.Count - 1);
                        tiles[kv.Key] = Int32.Parse(numbers[random]);
                        numbers.Remove(numbers[random]);
                    }
                }
            }

            // Sort Rows
            for (int i = 0; i < 9; i++)
            {
                if ((i + 1) != 3 && (i + 1) != 6 && (i + 1) != 9)
                {
                    for (int k = 0; k < 2; k++)
                    {

                        // Get Current Row
                        List<string> CurrentRow = new List<string>();
                        foreach (KeyValuePair<string, int> kv in tile)
                        {
                            if (kv.Key[1] == (i + 1).ToString()[0])
                            {
                                CurrentRow.Add(kv.Key);
                            }
                        }

                        // Get Rows Under 
                        List<string> RowsBelow = new List<string>();
                        for (int g = 0; g < 2; g++)
                        {
                            foreach (KeyValuePair<string, int> kv in tile)
                            {
                                if (kv.Key[1] == ((i + 1) + (g + 1)).ToString()[0])
                                {
                                    RowsBelow.Add(kv.Key);
                                }
                            }
                        }

                        // Get Duplicates
                        int counter = 0;
                        List<string> Originals = new List<string>();
                        List<string> Duplicates = new List<string>();
                        foreach (string Tile in CurrentRow)
                        {
                            counter++;
                            int offset = 0;
                            foreach (string uTile in CurrentRow)
                            {
                                if (offset >= counter)
                                {
                                    if (tiles[uTile] == tiles[Tile])
                                    {
                                        Originals.Add(Tile);
                                        Duplicates.Add(uTile);
                                        break;
                                    }
                                }
                                offset++;
                            }
                        }

                        // Get a list of values not in row
                        List<int> OldValues = new List<int>();
                        List<int> Values = new List<int>(); // List of values not in row
                        foreach (string Tile in CurrentRow)
                        {
                            OldValues.Add(tiles[Tile]);
                        }
                        foreach (char c in n)
                        {
                            if (!OldValues.Contains(Int32.Parse(c.ToString())))
                            {
                                Values.Add(Int32.Parse(c.ToString()));
                            }
                        }

                        int dCounter = 0;
                        foreach (string dupe in Duplicates)
                        {
                            int pass = 0;
                            foreach (string Tile in RowsBelow)
                            {
                                if (Tile[0] == dupe[0] && Values.Contains(tiles[Tile]))
                                {
                                    Values.Remove(tiles[Tile]);
                                    SwapValues(dupe, Tile);
                                    break;
                                }
                                else
                                {
                                    pass++;
                                }
                            }
                            if (pass == RowsBelow.Count)
                            {
                                string currentTile = Originals[dCounter];
                                foreach (string Tile in RowsBelow)
                                {
                                    if (Tile[0] == currentTile[0] && Values.Contains(tiles[Tile]))
                                    {
                                        Values.Remove(tiles[Tile]);
                                        SwapValues(currentTile, Tile);
                                        break;
                                    }
                                }
                            }
                            dCounter++;
                        }

                    }
                }
            }

            // Sort Columns
            for (int i = 0; i < 9; i++)
            {
                if ((i + 1) != 3 && (i + 1) != 6 && (i + 1) != 9)
                {
                    List<string> tried = new List<string>();
                    for (int k = 0; k < 9; k++)
                    {

                        // Get Current Columns
                        List<string> CurrentColumn = new List<string>();
                        foreach (KeyValuePair<string, int> kv in tile)
                        {
                            if (kv.Key[2] == (i + 1).ToString()[0])
                            {
                                CurrentColumn.Add(kv.Key);
                            }
                        }

                        // Get Columns to the Right 
                        List<string> ColumnsBelow = new List<string>();
                        for (int g = 0; g < 2; g++)
                        {
                            foreach (KeyValuePair<string, int> kv in tile)
                            {
                                if (kv.Key[2] == ((i + 1) + (g + 1)).ToString()[0])
                                {
                                    ColumnsBelow.Add(kv.Key);
                                }
                            }
                        }

                        // Get Duplicates
                        int counter = -1;
                        List<string> Originals = new List<string>();
                        List<string> Duplicates = new List<string>();
                        foreach (string Tile in CurrentColumn)
                        {
                            counter++;
                            int offset = -1;
                            foreach (string uTile in CurrentColumn)
                            {
                                if (offset >= counter)
                                {
                                    if (tiles[uTile] == tiles[Tile])
                                    {
                                        Originals.Add(Tile);
                                        Duplicates.Add(uTile);
                                        break;
                                    }
                                }
                                offset++;
                            }
                        }

                        // Get a list of values not in column
                        List<int> OldValues = new List<int>();
                        List<int> Values = new List<int>(); // List of values not in row
                        foreach (string Tile in CurrentColumn)
                        {
                            OldValues.Add(tiles[Tile]);
                        }
                        foreach (char c in n)
                        {
                            if (!OldValues.Contains(Int32.Parse(c.ToString())))
                            {
                                Values.Add(Int32.Parse(c.ToString()));
                            }
                        }

                        // Swap Numbers
                        int runCounter = 0;
                        // First Check
                        foreach (string dupe in Duplicates)
                        {
                            int pass = 0;
                            foreach (string uTile in ColumnsBelow)
                            {
                                if (uTile[0] == dupe[0] && uTile[1] == dupe[1])
                                {
                                    if (Values.Contains(tiles[uTile]))
                                    {
                                        pass++;
                                        SwapValues(uTile, dupe);
                                        break;
                                    }
                                }
                            }
                            // First Redundant Check
                            if (pass == 0)
                            {
                                foreach (string uTile in ColumnsBelow)
                                {
                                    if (uTile[0] == Originals[runCounter][0] && uTile[1] == Originals[runCounter][1])
                                    {
                                        if (Values.Contains(tiles[uTile]))
                                        {
                                            pass++;
                                            SwapValues(uTile, Originals[runCounter]);
                                            break;
                                        }
                                    }
                                }
                            }

                            /// Look for a tile in current column that has the value in its row and group - X
                            /// check if any available duplicates have that tiles value
                            /// if so then make the duplicate swap with that value and make the random tile swap with the value we need

                            // Second Redundant Check
                            if (pass == 0)
                            {
                                // Get Tile with value in its row and column
                                bool hasDuplicate = false;
                                string duplicate = "";
                                string vTile = "", kTile = ""; // vTile : the tile that contains the value, kTile : the tile with the vTile in its row and group
                                int x = 0;

                                foreach (string uTile in CurrentColumn)
                                {
                                    if (!tried.Contains(uTile))
                                    {
                                        foreach (string bTile in ColumnsBelow)
                                        {
                                            if (Values.Contains(tiles[bTile]) && bTile[0] == uTile[0] && bTile[1] == uTile[1] && x == 0)
                                            {
                                                x++;
                                                vTile = bTile;
                                                kTile = uTile;
                                                break;
                                            }
                                        }
                                    }
                                }

                                foreach (string uTile in CurrentColumn)
                                {
                                    if (uTile != kTile && tiles[kTile] == tiles[uTile])
                                    {
                                        hasDuplicate = true;
                                        duplicate = uTile;
                                    }
                                }

                                if (hasDuplicate)
                                {
                                    SwapValues(duplicate, vTile);
                                }
                                else
                                {
                                    SwapValues(kTile, vTile);
                                    tried.Add(kTile);
                                }

                            }
                            runCounter++;
                        }

                    }
                }
            }

            // Write Seed to Readable Format
            foreach (KeyValuePair<string, int> key in tile) {
                seed = seed + tiles[key.Key].ToString();
            }

            return seed;

        }


        public string GenerateSeed()
        {

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string newSeed = "";
            List<KeyValuePair<string, int>> tile = tiles.ToList();
            List<int> duplicates = new List<int>();
            int check = 0;
            string group = "abcdefghi";

            bool usableSeed = false;
            while (!usableSeed) {

                SeedGenerator(tile);

                check = 0;
                foreach (char g in group) {
                    duplicates = new List<int>();
                    foreach (KeyValuePair<string, int> kv in tile) {
                        if (kv.Key[0] == g) {
                            if (!duplicates.Contains(tiles[kv.Key])) {
                                duplicates.Add(tiles[kv.Key]);
                                check++;
                            }
                            else {
                                break;
                            }
                        }
                    }
                }

                if (check == 81) usableSeed = true;

            }

            watch.Stop();
            generationTime = watch.ElapsedMilliseconds;

            // Write Seed to Readable Format
            foreach (KeyValuePair<string, int> key in tile)
            {
                newSeed = newSeed + tiles[key.Key].ToString();
            }

            return newSeed;
        }

        public string FinishSeed(string level)
        {
            List<KeyValuePair<string, int>> tile = tiles.ToList();
            int difficulty = 81;
            if (debug)
            {
                if (gm_easy) { difficulty -= rnd.Next(20, 25); }
                if (gm_normal) { difficulty -= rnd.Next(17, 20); }
                if (gm_hard) { difficulty -= rnd.Next(11, 17); }

                for (int i = 0; i < difficulty; i++)
                {
                    int randTile = rnd.Next(0, 81);
                    tiles[tile[randTile].Key] = 0;
                }

                // Write Seed to Readable Format
                level = "";
                foreach (KeyValuePair<string, int> key in tile)
                {
                    level = level + tiles[key.Key].ToString();
                    tiles[key.Key] = 0;
                }
            }
            return level;
        }
        public void SwapValues(string key1, string key2)
        {
            int first = 0, second = 0;
            first = tiles[key1];
            second = tiles[key2];

            tiles[key1] = second;
            tiles[key2] = first;
        }

        // Win / Loss Functions
        public void Win()
        {
            usingTimer = false;
            string message = " You filled every tile with the correct number ! ";
            string title = " You Won! ";
            MessageBoxButton buttons = MessageBoxButton.OK;
            MessageBoxResult result = MessageBox.Show(message, title, buttons);
            if (result == MessageBoxResult.OK)
            {
                open_scene(game, main_menu);
            }

        }
        public void Lose(string Message)
        {
            usingTimer = false;
            string message = Message;
            string title = " You Lost! ";
            MessageBoxButton buttons = MessageBoxButton.OK;
            MessageBoxResult result = MessageBox.Show(message, title, buttons);
            if (result == MessageBoxResult.OK)
            {
                open_scene(game, main_menu);
            }
            open_scene(game, main_menu);
        }

        // Board Reset
        public void reset()
        {

            if (time <= 0)
            {
                usingTimer = false;
            }
            if (usingUndo)
            {
                UndosLabel.Content = "Undos : " + Undos.ToString();
            }
            else
            {
                Undos = 1;
                UndosLabel.Content = "Undos : Infinite";
            }
            List<KeyValuePair<string, int>> tile = tiles.ToList();
            StartTimer();

            string seed = "";
            if (currentLevel == 0) { solvedBoards[0] = GenerateSeed(); boards[0] = FinishSeed(solvedBoards[0]); }
            seed = boards[currentLevel];
            Generation_Time.Content = "Generation Time : " + generationTime.ToString() + "ms";

            int counter = 0;
            foreach (string key in tiles.Keys.ToList())
            {
                if (seed[counter].ToString() != "0")
                {
                    tiles[key] = Int32.Parse(seed[counter].ToString());
                    object button = FindName(key);
                    (button as Button).Content = tiles[key].ToString();
                }
                else
                {
                    object button = FindName(key);
                    (button as Button).Content = "";
                }
                counter++;
            }
        }

        // Menu Options [ Main Menu ]
        private void menu_button_click(object sender, RoutedEventArgs e)
        {
            string name = (sender as Button).Name;
            if (name == "play_button")
            {
                open_scene(main_menu, mode_selection);
            }
            if (name == "about_button")
            {
                open_scene(main_menu, about_section);
            }
            if (name == "exit_button")
            {
                exit_window();
            }
            if (name == "about_back_button")
            {
                open_scene(about_section, main_menu);
            }
            if (name == "back_button")
            {
                open_scene(mode_selection, main_menu);
            }
            if (name == "play_button1")
            {
                open_scene(mode_selection, game);
                reset();
            }
            if (name == "game_quit")
            {
                open_scene(game, main_menu);
                timer.Stop();
            }
        }
        private void level_button_click(object sender, RoutedEventArgs e)
        {
            string name = (sender as Button).Name;
            if (name == "Level1") { currentLevel = 1; CurrentLevelText.Content = "Selected Level : Level 1"; }
            if (name == "Level2") { currentLevel = 2; CurrentLevelText.Content = "Selected Level : Level 2"; }
            if (name == "Level3") { currentLevel = 3; CurrentLevelText.Content = "Selected Level : Level 3"; }
            if (name == "Level4") { currentLevel = 4; CurrentLevelText.Content = "Selected Level : Level 4"; }
            if (name == "Level5") { currentLevel = 5; CurrentLevelText.Content = "Selected Level : Level 5"; }
            if (name == "Level6") { currentLevel = 6; CurrentLevelText.Content = "Selected Level : Level 6"; }
            if (name == "Level7") { currentLevel = 7; CurrentLevelText.Content = "Selected Level : Level 7"; }
            if (name == "Level8") { currentLevel = 8; CurrentLevelText.Content = "Selected Level : Level 8"; }
            if (name == "Level9") { currentLevel = 9; CurrentLevelText.Content = "Selected Level : Level 9"; }
            if (name == "Random")
            {
                currentLevel = 0;
                CurrentLevelText.Content = "Selected Level : Random";
                isEasy.IsEnabled = true;
                isNormal.IsEnabled = true;
                isHard.IsEnabled = true;
            }
            else
            {
                isEasy.IsEnabled = false;
                isNormal.IsEnabled = false;
                isHard.IsEnabled = false;
            }
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sender as Slider).Name == "TimerSlider")
            {
                if (((float)Math.Round((sender as Slider).Value, 1)) > 0)
                {
                    usingTimer = true;
                    Timer.IsChecked = true;
                }
                else { usingTimer = false; Timer.IsChecked = false; }
                time = ((float)Math.Round((sender as Slider).Value, 1));
                Timer.Content = "Time : " + time.ToString() + " Minutes";
                time = time * 60f;
            }
            if ((sender as Slider).Name == "UndoSlider")
            {
                if (((int)(sender as Slider).Value) > 0)
                {
                    usingUndo = true;
                    Undo.IsChecked = true;
                }
                else { usingUndo = false; Undo.IsChecked = false; }
                Undos = ((int)(sender as Slider).Value);
                Undo.Content = "Undos : " + Undos.ToString();
            }
        }

        // Difficulty and Options
        private void GameMode_Checked(object sender, RoutedEventArgs e)
        {
            string name = (sender as RadioButton).Name;
            if (name == "isEasy") { gm_easy = (sender as RadioButton).IsChecked.Value; gm_normal = false; gm_hard = false; }
            if (name == "isNormal") { gm_normal = (sender as RadioButton).IsChecked.Value; gm_hard = false; gm_easy = false; }
            if (name == "isHard") { gm_hard = (sender as RadioButton).IsChecked.Value; gm_normal = false; gm_easy = false; }
        }
        private void timer_check_Checked(object sender, RoutedEventArgs e)
        {
            usingTimer = (sender as CheckBox).IsChecked.Value;
        }
        private void Undo_Checked(object sender, RoutedEventArgs e)
        {
            usingUndo = (sender as CheckBox).IsChecked.Value;
        }

        // Checks
        public bool CheckPreset(object sender)
        {
            bool x = false;
            int counter = 0;
            foreach (string Key in tiles.Keys)
            {
                counter++;
                if (Key == (sender as Button).Name)
                {
                    if (solvedBoards[currentLevel][counter - 1] == currentNumber.ToString()[0])
                    {
                        x = true;
                    }
                    else if (Undos >= 1 && tiles[Key] == 0)
                    {
                        string message = "";
                        if (usingUndo)
                        {
                            message = " That was the wrong move, you've used a Undo | Undos Left : " + Undos.ToString();
                        }
                        else
                        {
                            message = " That was the wrong move";
                        }
                        string title = "Move Undone ";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxResult result = MessageBox.Show(message, title, buttons);
                        if (result == MessageBoxResult.OK)
                        {
                            if (usingUndo)
                            {
                                Undos--;
                            }

                            UndoMove((sender as Button).Name);
                            if (usingUndo)
                            {
                                UndosLabel.Content = "Undos : " + Undos.ToString();
                            }
                            else
                            {
                                UndosLabel.Content = "Undos : Infinite";
                            }

                            break;
                        }

                        x = false;
                    }
                    else
                    {
                        if (Undos <= 0 && tiles[Key] == 0)
                        {
                            UndosLabel.Content = "Undos : " + Undos.ToString();
                            Lose(" You ran out of undos! ");
                        }
                        break;
                        x = false;
                    }
                }
            }

            return x;

        }
        public void UndoMove(string key)
        {
            tiles[key] = 0;
        }

        // Timer Stuff
        public void StartTimer()
        {
            if (usingTimer)
            {
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                timer.Interval = 1000;
                timer.Enabled = usingTimer;
            }
            else
            {
                TimeLeft.Content = "-- : --";
            }

        }
        public void UpdateTimer()
        {
            TimeLeft.Content = currentTime;
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (time > 0)
            {
                time--;
                int sec = 0, min = 0;
                for (int i = 0; i < time; i++)
                {
                    sec++;
                    if (sec == 60)
                    {
                        sec = 0;
                        min++;
                    }
                }
                currentTime = min.ToString() + " : " + sec.ToString();
                this.Dispatcher.Invoke(() => {
                    UpdateTimer();
                });
            }
            else
            {
                this.Dispatcher.Invoke(() => {
                    timer.Stop();
                    Lose(" You ran out of time! ");
                });
            }
        }

        // Check if player has Won
        public bool CheckWin()
        {

            bool x = false;
            int counter = 0;

            foreach (int i in tiles.Values)
            {
                if (i != 0)
                {
                    counter++;
                }
            }

            if (counter == 81)
            {
                x = true;
            }

            return x;
        }

        // Places number if slot is empty
        public void PlaceNumber(bool v, bool h, bool g, object sender)
        {
            if ((sender as Button).Content == "" && v && h && g)
            {
                tiles[(sender as Button).Name] = currentNumber;
                (sender as Button).Content = tiles[(sender as Button).Name].ToString();
            }
        }

        // Runs the Necessary Code on Tile Click
        private void OnTileClick(object sender, RoutedEventArgs e)
        {

            bool x = CheckPreset(sender);
            PlaceNumber(x, x, x, sender);
            if (CheckWin())
            {
                Win();
            }
        }

        // Select Number
        private void OnNumberPicked(object sender, RoutedEventArgs e)
        {
            string name = (sender as RadioButton).Name;
            if (name == "number1")
            {
                currentNumber = 1;
            }
            if (name == "number2")
            {
                currentNumber = 2;
            }
            if (name == "number3")
            {
                currentNumber = 3;
            }
            if (name == "number4")
            {
                currentNumber = 4;
            }
            if (name == "number5")
            {
                currentNumber = 5;
            }
            if (name == "number6")
            {
                currentNumber = 6;
            }
            if (name == "number7")
            {
                currentNumber = 7;
            }
            if (name == "number8")
            {
                currentNumber = 8;
            }
            if (name == "number9")
            {
                currentNumber = 9;
            }
        }
    }
}

