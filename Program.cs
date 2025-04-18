using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ConsoleApp_TextRPG_SpartaDungeon

{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var game = new Game();
            game.Start();
        }
    }

    // 데이터 저장용 DTO
    public class GameState
    {
        public CharacterState Player { get; set; }
        public List<ItemState> Inventory { get; set; }
        public float Health { get; set; }
        public float Gold { get; set; }
    }

    public class CharacterState
    {
        public string Name { get; set; }
        public string Job { get; set; }
        public int Level { get; set; }
        public float BaseAttack { get; set; }
        public float BaseDefense { get; set; }
    }

    public class ItemState
    {
        public string Name { get; set; }
        public float Attack { get; set; }
        public float Defense { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public int Type { get; set; }
        public bool Purchased { get; set; }
        public bool Equipped { get; set; }
    }


    public class Game
    {
        private Character _player;
        private Shop _shop;
        private Rest _rest;
        private Dungeon _dungeon;
        private const string SaveFile = "TextRPG_Sparta.json";



        public void Start()
        {
            // 저장 파일이 있으면 불러오고, 없으면 새 게임
            if (File.Exists(SaveFile)) LoadGame();
            else InitNewGame();


            // 서비스 객체 초기화
            _shop = new Shop(_player);
            _rest = new Rest(_player);
            _dungeon = new Dungeon(_player);
            MainLoop();
        }


        private void LoadGame()
        {
            try
            {
                string json = File.ReadAllText(SaveFile);
                var state = JsonSerializer.Deserialize<GameState>(json);
                if (state == null) throw new Exception("세이브 데이터 파싱 실패");

                _player = new Character(state.Player.Name, state.Player.Job)
                {
                    Level = state.Player.Level,
                    BaseAttack = state.Player.BaseAttack,
                    BaseDefense = state.Player.BaseDefense,
                    Health = state.Health,
                    Gold = state.Gold
                };

                foreach (var itemState in state.Inventory)
                {
                    var item = new Item(
                        itemState.Name,
                        itemState.Attack,
                        itemState.Defense,
                        itemState.Description,
                        itemState.Price,
                        itemState.Type)
                    {
                        Purchased = itemState.Purchased,
                        Equipped = itemState.Equipped
                    };
                    _player.Inventory.AddItem(item);
                }

                Console.WriteLine("세이브 데이터를 불러왔습니다.");
                Pause();
            }
            catch
            {
                Console.WriteLine("로드 오류 발생, 새 게임을 시작합니다.");
                Pause();
                InitNewGame();
            }
        }

        private void SaveGame()
        {
            var state = new GameState
            {
                Player = new CharacterState
                {
                    Name = _player.Name,
                    Job = _player.Job,
                    Level = _player.Level,
                    BaseAttack = _player.BaseAttack,
                    BaseDefense = _player.BaseDefense
                },
                Health = _player.Health,
                Gold = _player.Gold,
                Inventory = _player.Inventory._items.Select(i => new ItemState
                {
                    Name = i.Name,
                    Attack = i.Attack,
                    Defense = i.Defense,
                    Description = i.Description,
                    Price = i.Price,
                    Type = i.Type,
                    Purchased = i.Purchased,
                    Equipped = i.Equipped
                }).ToList()
            };

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(state, options);
                File.WriteAllText(SaveFile, json);
            }
            catch
            {
                Console.WriteLine("저장 오류 발생");
            }
        }


        private void InitNewGame()
        {
            Console.WriteLine("스파르타 던전에 오신 여러분 환영합니다!");
            Console.Write("이름을 입력해주세요: ");
            string name = Console.ReadLine()?.Trim() ?? "Chad";

            Console.WriteLine("원하는 직업을 선택하세요: 1. 전사  2. 도적");
            string job = Console.ReadLine() == "2" ? "도적" : "전사";

            _player = new Character(name, job);
        }



        private void MainLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
                Console.WriteLine("이곳에서 던전으로 들어가기 전 활동을 할 수 있습니다.\n");
                Console.WriteLine("1. 상태 보기");
                Console.WriteLine("2. 인벤토리");
                Console.WriteLine("3. 상점");
                Console.WriteLine("4. 휴식하기");
                Console.WriteLine("5. 던전 입장\n");
                Console.Write("원하시는 행동을 입력해주세요: ");
                switch (Console.ReadLine())
                {
                    case "1": _player.ShowStatus(); break;
                    case "2": _player.InventoryMenu(); break;
                    case "3": _shop.ShopMenu(); break;
                    case "4": _rest.RestAction(); break;
                    case "5": _dungeon.Enter(); break;
                    default:
                        Console.WriteLine("잘못된 입력입니다.");
                        Pause();
                        break;
                }
                SaveGame();
            }
        }

        public static void Pause()
        {
            Console.WriteLine("\n계속하려면 아무 키나 누르세요...");
            Console.ReadKey();
        }
    }

    public class Dungeon
    {
        private readonly Character _player;
        private readonly Random _rand = new Random();
        private readonly (string Name, float RecDef, float BaseGold)[] _levels = new[]
        {
            ("쉬운 던전", 5f, 1000f),
            ("일반 던전",11f,1700f),
            ("어려운 던전",17f,2500f)
        };

        public Dungeon(Character player) => _player = player;

        public void Enter()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("던전 입장\n");

                if (_player.Health <= 0f)
                {
                    Console.WriteLine("체력이 0이 되어 더 이상 던전을 진행할 수 없습니다.");
                    Game.Pause();
                    return;
                }
                else
                {
                    for (int i = 0; i < _levels.Length; i++)
                    {
                        var lv = _levels[i];
                        Console.WriteLine($"{i + 1}. {lv.Name} | 방어력 {lv.RecDef} 이상 권장");
                    }
                    Console.WriteLine("0. 나가기\n");
                    Console.Write("선택: ");
                    if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > _levels.Length)
                    {
                        Console.WriteLine("잘못된 입력입니다."); Game.Pause(); continue;
                    }
                    if (choice == 0) return;
                    RunLevel(choice - 1);
                    return;
                }
            }

        }

        private void RunLevel(int idx)
        {
            var (name, recDef, baseGold) = _levels[idx];
            Console.Clear();
            float playerDef = _player.BaseDefense + _player.Inventory.EquippedDefenseBonus();
            bool isOk = playerDef >= recDef; //권장 방어력보다 높은가? 높으면 true
            float diff = recDef - playerDef;

            float min = Math.Max(0f, 20f + diff);
            float max = 35f + diff;
            int dmgMin = (int)min;
            int dmgMax = (int)max;
            int dmgInt = _rand.Next(dmgMin, dmgMax + 1);
            float damage = dmgInt;

            bool clear = true;
            if (!isOk && _rand.Next(100) < 40)
            {
                clear = false;
                _player.Health /= 2f;
            }

            float oldHp = _player.Health;
            float oldG = _player.Gold;
            _player.Health = Math.Max(0f, _player.Health - damage);

            float gain = 0f;
            if (clear)
            {
                _player.LevelManager.RegisterDungeonClear();
                float atk = _player.BaseAttack + _player.Inventory.EquippedAttackBonus();
                int per = _rand.Next((int)atk, (int)(atk * 2f) + 1);
                gain = baseGold + baseGold * per / 100f;
                _player.Gold += gain;
            }

            Console.WriteLine(clear ? "던전 클리어! 축하합니다!!" : "던전 실패... 아쉽군요...");
            Console.WriteLine($"[{name} 탐험 결과]\n");
            Console.WriteLine($"체력: {oldHp} -> {_player.Health}");
            Console.WriteLine($"Gold: {oldG} -> {_player.Gold}");
            Console.WriteLine("\n0. 나가기"); Console.ReadLine();
        }
    }

    public class Character
    {
        public string Name { get; }
        public string Job { get; }
        public int Level { get; set; } = 1;
        public float BaseAttack { get; set; } = 10f;
        public float BaseDefense { get; set; } = 5f;
        public float Health { get; set; } = 100f;
        public float Gold { get; set; } = 15000f;

        public float DungeonClears { get; set; } = 0f;
        public LevelManager LevelManager { get; }
        public Inventory Inventory { get; }

        public Character(string name, string job)
        {
            Name = name;
            Job = job;
            Inventory = new Inventory(this);
            LevelManager = new LevelManager(this);
        }

        public void ShowStatus()
        {
            while (true)
            {
                Console.Clear();
                float bAtk = Inventory.EquippedAttackBonus();
                float bDef = Inventory.EquippedDefenseBonus();

                Console.WriteLine("상태 보기\n");
                Console.WriteLine($"Lv. {Level:00} ");
                Console.WriteLine($"{Name} ( {Job} )");
                Console.WriteLine($"공격력 : {BaseAttack + bAtk} {(bAtk > 0 ? $"(+{bAtk})" : "")}");
                Console.WriteLine($"방어력 : {BaseDefense + bDef} {(bDef > 0 ? $"(+{bDef})" : "")}");
                Console.WriteLine($"체  력 : {Health}");
                Console.WriteLine($"Gold : {Gold}\n");

                Console.WriteLine("0. 나가기");
                if (Console.ReadLine() == "0") break;
                Console.WriteLine("잘못된 입력입니다."); Game.Pause();
            }
        }

        public void InventoryMenu() => Inventory.Menu();
        public void AddDungeonClear() => DungeonClears++;
        public void IncreaseStats() { BaseAttack += 0.5f; BaseDefense += 1f; }
    }

    public class LevelManager
    {
        private readonly Character _c;
        public LevelManager(Character c) => _c = c;
        public void RegisterDungeonClear()
        {
            _c.AddDungeonClear(); Console.WriteLine("던전 클리어 횟수가 1 증가했습니다.");
            while (_c.DungeonClears >= _c.Level)
            {
                _c.DungeonClears -= _c.Level;
                _c.IncreaseStats();
                _c.GetType().GetProperty("Level").SetValue(_c, _c.Level + 1);
                Console.WriteLine($"레벨업! 새로운 레벨: {_c.Level}");
            }
        }
    }

    public class Inventory
    {
        private readonly Character _owner;
        public readonly List<Item> _items = new List<Item>();
        public Inventory(Character owner) => _owner = owner;
        public void AddItem(Item item) => _items.Add(item);
        public void DeleteItem(Item item) => _items.Remove(item);
        public float EquippedAttackBonus() => _items.Where(i => i.Equipped).Sum(i => i.Attack);
        public float EquippedDefenseBonus() => _items.Where(i => i.Equipped).Sum(i => i.Defense);

        public void Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("인벤토리\n[아이템 목록]");
                if (_items.Count == 0) Console.WriteLine("- 보유한 아이템이 없습니다.");
                else
                {
                    for (int i = 0; i < _items.Count; i++)
                    {
                        var it = _items[i];
                        string tag = it.Equipped ? "[E]" : "";
                        Console.WriteLine($"{i + 1}. {tag}{it.Name,-14} | {it.StatString()} | {it.Description}");
                    }
                }
                Console.WriteLine("\n1. 장착 관리  0. 나가기");
                Console.Write("선택: ");
                var inp = Console.ReadLine();
                if (inp == "0") return;

                else if (inp == "1") ManageEquip(); else { Console.WriteLine("잘못된 입력"); Game.Pause(); }
            }
        }

        private void ManageEquip()
        {
            while (true)
            {
                Console.Clear(); Console.WriteLine("장착 관리\n");
                for (int i = 0; i < _items.Count; i++)
                {
                    var it = _items[i]; string tag = it.Equipped ? "[E]" : "";
                    Console.WriteLine($"{i + 1}. {tag}{it.Name,-14} | {it.StatString()} | {it.Description}");
                }
                Console.WriteLine("\n0. 나가기"); Console.Write("선택: ");
                var inp = Console.ReadLine();



                if (inp == "0")
                {
                    return;
                }
                else
                {
                    if (!int.TryParse(inp, out int c) || c < 1 || c > _items.Count)
                    {
                        Console.WriteLine("잘못된 입력"); Game.Pause(); continue;
                    }
                    var sel = _items[c - 1];
                    if (!sel.Equipped)
                    {
                        var conflict = _items.FirstOrDefault(x => x.Type == sel.Type && x.Equipped);
                        if (conflict != null) { conflict.Equipped = false; Console.WriteLine($"{conflict.Name} 해제"); }
                        sel.Equipped = true; Console.WriteLine($"{sel.Name} 장착");
                    }
                    else { sel.Equipped = false; Console.WriteLine($"{sel.Name} 해제"); }

                    Game.Pause();
                }



            }
        }
    }

    public class Shop
    {
        private readonly Character _buyer;
        private readonly List<Item> _items;
        public Shop(Character buyer)
        {
            _buyer = buyer;
            _items = new List<Item>
            {
                new Item("수련자 갑옷",0,5,"수련에 도움을 주는 갑옷입니다.",1000f,2),
                new Item("무쇠갑옷",0,9,"무쇠로 만들어져 튼튼한 갑옷입니다.",1500f,2),
                new Item("스파르타의 갑옷",0,15,"스파르타 전설 갑옷입니다.",3500f,2),
                new Item("낡은 검",2,0,"낡은 검입니다.",600f,1),
                new Item("청동 도끼",5,0,"청동 도끼입니다.",1500f,1),
                new Item("스파르타의 창",7,0,"스파르타 전설 창입니다.",2500f,1),
                new Item("스파르타의 검",10,0,"스파르타 전설 검입니다.",4500f,1)
            };
        }

        public void ShopMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"상점\n[Gold: {_buyer.Gold}]\n");
                for (int i = 0; i < _items.Count; i++)
                {
                    var it = _items[i];
                    string status = it.Purchased ? "구매완료" : $"{it.Price}";
                    Console.WriteLine($"{i + 1}. {it.Name,-14} | {it.StatString()} | {status}");
                }
                Console.WriteLine("\n1. 구매  2. 판매  0. 나가기"); Console.Write("선택: ");
                var inp = Console.ReadLine();
                if (inp == "0") return;
                if (inp == "1") BuyMenu(); else if (inp == "2") SellMenu(); else { Console.WriteLine("잘못된 입력"); Game.Pause(); }
            }
        }

        private void BuyMenu()
        {
            while (true)
            {
                Console.Clear(); Console.WriteLine($"구매\n[Gold: {_buyer.Gold}]\n");
                for (int i = 0; i < _items.Count; i++)
                {
                    var it = _items[i];
                    string status = it.Purchased ? "구매완료" : $"{it.Price}";
                    Console.WriteLine($"{i + 1}. {it.Name,-14} | {it.StatString()} | {status}");
                }
                Console.WriteLine("\n0. 나가기"); Console.Write("선택: ");
                if (!int.TryParse(Console.ReadLine(), out int c) || c < 0 || c > _items.Count) { Console.WriteLine("잘못된 입력"); Game.Pause(); continue; }
                if (c == 0) return;
                var sel = _items[c - 1];
                if (sel.Purchased) Console.WriteLine("이미 구매함");
                else if (_buyer.Gold >= sel.Price)
                {
                    _buyer.Gold -= sel.Price; sel.Purchased = true; _buyer.Inventory.AddItem(sel);
                    Console.WriteLine("구매완료");
                }
                else Console.WriteLine("Gold 부족");
                Game.Pause();
            }
        }

        private void SellMenu()
        {
            while (true)
            {
                Console.Clear(); var inv = _buyer.Inventory._items;
                Console.WriteLine($"판매\n[Gold: {_buyer.Gold}]\n");
                for (int i = 0; i < inv.Count; i++)
                {
                    var it = inv[i];
                    float p = it.Price * 0.85f;
                    Console.WriteLine($"{i + 1}. {it.Name,-14} | {(int)p}");
                }
                Console.WriteLine("\n0. 나가기"); Console.Write("선택: ");
                if (!int.TryParse(Console.ReadLine(), out int c) || c <= 0 || c > inv.Count) { Console.WriteLine("잘못된 입력"); Game.Pause(); continue; }
                var sold = inv[c - 1]; _buyer.Gold += sold.Price * 0.85f;
                _items.First(x => x.Name == sold.Name).Purchased = false; inv.Remove(sold);
                Console.WriteLine("판매완료"); Game.Pause();
            }
        }
    }

    public class Item
    {
        public string Name { get; }
        public float Attack { get; }
        public float Defense { get; }
        public string Description { get; }
        public float Price { get; }
        public int Type { get; }
        public bool Purchased { get; set; }
        public bool Equipped { get; set; }

        public Item(string name, float atk, float def, string desc, float price, int type)
        {
            Name = name; Attack = atk; Defense = def; Description = desc; Price = price; Type = type;
        }

        public string StatString()
        {
            if (Attack > 0) return $"공격력 +{Attack}";
            if (Defense > 0) return $"방어력 +{Defense}";
            return string.Empty;
        }
    }

    public class Rest
    {
        private readonly Character Player;
        private const float Cost = 500f;
        public Rest(Character player) => Player = player;
        public void RestAction()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"휴식 (Cost: {Cost}, Gold: {Player.Gold})");
                Console.WriteLine("1. 휴식하기  0. 나가기"); Console.Write("선택: ");
                string inp = Console.ReadLine();
                if (inp == "0") return;
                if (inp == "1")
                {
                    if (Player.Gold >= Cost) { Player.Gold -= Cost; Player.Health = 100f; Console.WriteLine("휴식완료"); }
                    else Console.WriteLine("Gold 부족");
                }
                else Console.WriteLine("잘못된 입력");
                Game.Pause();
            }
        }
    }
}