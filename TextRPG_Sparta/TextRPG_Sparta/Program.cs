using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;//저장
using System.Xml.Serialization;//저장
using static System.Net.Mime.MediaTypeNames;
using static TextRPG_Sparta.User;

namespace TextRPG_Sparta
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameManager gameManager = new GameManager();
            User player = new User(0);

            // 게임 로드
            gameManager.LoadGame(player);

            // 기본 값 설정
            if (player.Gold == 0) // 세이브된 데이터가 없으면 초기화
            {
                player.Gold = 15000;
                gameManager.FirstUserSettings(player);
            }

            List<Weapon> inventory = new List<Weapon>();
            List<Weapon> userInventory = new List<Weapon>();
            userInventory.Add(new Weapon("낡은 검", 2, 0, 600,1, false, "쉽게 볼 수 있는 낡은 검 입니다."));//이름, 공격, 방어, 가격,type(아이템 유형), 장착여부, 스토리
            userInventory.Add(new Weapon("청동 도끼", 5, 0, 1500,1, false, "값이 조금 나가보이는 도끼입니다."));// 1이면 무기 2이면 방어구
            userInventory.Add(new Weapon("스파르타의 창", 7, 0, 2100,1, false, "스파르타의 전사들이 사용했다는 전설의 창입니다."));
            userInventory.Add(new Weapon("수련자의 갑옷", 0, 5, 1000,2, false, "아주 단단한 갑옷입니다."));
            userInventory.Add(new Weapon("무쇠 갑옷", 0, 9, 2000,2, false,"무쇠로 만들어져 튼튼한 갑옷입니다."));
            userInventory.Add(new Weapon("스파르타의 갑옷", 0, 15, 3500, 2, false, "스파르타 전사들이 아끼던 갑옷입니다."));
            inventory.Add(new Weapon("가성비 갑옷", 0, 3, 300,2, false, "없는 것 보다는 낫습니다."));
            //inventory[0].ItemList(inventory, "활성화확인");
            gameManager.InVillage(player, inventory, userInventory);
        }
    }
    public interface ICharacter
    {
        string Name { get; set; }
        float Health { get; set; }
        float Attack { get; set; }
        float Defend { get; set; }
        int Gold { get; set; }
        bool IsDead { get; set; }
        public void CharacterInfo();
    }
    [Serializable]
    public class GameData
    {
        public int PlayerLevel;
        public int PlayerGold;
        public string PlayerName;

        // 저장 메서드
        public void SaveGameData(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }

        // 로드 메서드
        public static GameData LoadGameData(string filePath)
        {
            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GameData));
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    return (GameData)serializer.Deserialize(stream);
                }
            }
            return null; // 파일이 없을 경우 null 반환
        }
    }
    public class User : GameManager, ICharacter
    {
        public string Name { get; set; }
        public string job;
        public int level {  get; set; }
        public int clearMap {  get; set; }
        public float Health { get; set; }
        public float Attack { get; set; }
        public float Defend { get; set; }
        public int Gold { get; set; }
        public bool IsDead { get; set; }

        public User(int clearMap1)
        {           
            Name = "미지의플레이어"; job = "무직";
            Health = 100f; Attack = 10f; Defend = 5f; Gold = 0; IsDead = false; level = clearMap1+1; clearMap = clearMap1;
        }
        public void CharacterInfo()
        {
            Console.WriteLine($"상태 보기\r\n캐릭터의 정보가 표시됩니다.\n\n이름 : {Name}\n레벨 : {level}\nChad : {job}\n공격력 : {Attack}\n방어력 : {Defend}\n체력 : {Health}\nGold : {Gold}");
            Console.WriteLine("\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
            int input = int.Parse(Console.ReadLine());
            Console.Clear();
        }
        public void Rest(User user)
        {
            Console.WriteLine($"휴식하기\r\n500 G 를 내면 체력을 회복할 수 있습니다. (보유 골드 : {this.Gold} G)\r\n\r\n1. 휴식하기\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
            int input = int.Parse(Console.ReadLine());
            if (input == 0) goBackVillage();
            else if(input == 1)
            {
                if (user.Health >= 100)
                {
                    Console.WriteLine("→ 체력이 FULL입니다.");
                    goBackVillage();
                }
                else if (user.Gold >= 500)
                {
                    user.Gold -= 500;
                    user.Health = 100;
                    Console.WriteLine("→ 휴식을 완료했습니다.");
                    goBackVillage();
                }
                else
                {
                    Console.WriteLine("→ Gold 가 부족합니다. ");
                    goBackVillage();
                }
            }
        }
        public void LevelUp(User user)
        {
            level += 1;
            user.Attack += (user.level / 2);
            user.Defend = (user.level);
            Console.WriteLine("레벨업을 하였습니다!");
            Thread.Sleep(2000);
        }
    }

    public  class GameManager
    {
        private const string saveFilePath = "savedGame.xml"; // 세이브 파일 경로

        public void SaveGame(User user)
        {
            GameData gameData = new GameData
            {
                PlayerLevel = user.level,
                PlayerGold = user.Gold,
                PlayerName = user.Name
            };
            gameData.SaveGameData(saveFilePath);
            Console.WriteLine("게임이 저장되었습니다.");
        }

        public void LoadGame(User user)
        {
            GameData loadedData = GameData.LoadGameData(saveFilePath);
            if (loadedData != null)
            {
                user.level = loadedData.PlayerLevel;
                user.Gold = loadedData.PlayerGold;
                user.Name = loadedData.PlayerName;
                Console.WriteLine("게임이 로드되었습니다.");
            }
            else
            {
                Console.WriteLine("세이브 파일이 존재하지 않습니다.");
            }
        }
        public void goBackVillage()
        {
            Console.WriteLine("마을로 돌아가는중...");
            Thread.Sleep(2000);
        }
        
        public void FirstUserSettings(User user) //처음 캐릭터 정보 입력
        {
            Console.WriteLine("용사의 이름은 무엇입니까? >>");
            string UserName = Console.ReadLine();
            user.Name = UserName;
            Console.WriteLine($"{UserName}님의 직업은 무엇입니까? >>");
            string UserJob = Console.ReadLine();
            user.job = UserJob;
        }

        public void InVillage(User user, List<Weapon> inventory, List<Weapon> userInventory)
        {

        InVillage:
            Console.Clear();
            Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.\r\n이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\n");
            Console.WriteLine("1. 상태보기\n2. 인벤토리\n3. 상점\n4. 던전 들어가기\n5. 휴식하기\n6. 게임저장\n7. 로드게임 \n\n원하시는 행동을 입력해주세요\n>>");
            int userInput = int.Parse(Console.ReadLine());
            Console.Clear();
            switch (userInput)
            {
                case 1:
                    user.CharacterInfo();
                    goto InVillage;
                case 2:
                    inventory[0].ItemInfo(user, userInventory);
                    goto InVillage;
                case 3:
                    inventory[0].MarketInfo(user, inventory, userInventory);//  inventory[0]가 없을때 생기는 문제
                    goto InVillage;
                case 4:
                    VillageToDungeon(user);
                    goto InVillage;
                case 5:
                    user.Rest(user);
                    goto InVillage;
                case 6:
                    SaveGame(user);
                    goto InVillage;
                case 7:
                    LoadGame(user);
                    goto InVillage;
                default:
                    Console.WriteLine("게임을 종료합니다.");
                    break;
            }
            Thread.Sleep(2000);

        }

        public void VillageToDungeon(User user)
        { 
        Console.Clear();
        Console.WriteLine("던전입장\r\n이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\r\n\r\n1. 쉬운 던전     | 방어력 5 이상 권장\r\n2. 일반 던전     | 방어력 11 이상 권장\r\n3. 어려운 던전    | 방어력 17 이상 권장\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
        int input = int.Parse(Console.ReadLine());
           if(input >0&& input<=3) CreateDungeon(user, input);       
        }
        public void CreateDungeon(User user, int type)//난이도 : 1쉬움 2보통 3어려움
        {
            Random random = new Random();

            int randDef = random.Next(20, 36);
            int FailRandom = random.Next(0, 100);
            int goldSet = 0; int needDef = 5;
           
            if (type == 1)
            {
                needDef = 5;
                goldSet = 1000;
            }
            else if (type == 2)
            {
                needDef = 11;
                goldSet = 1700;

            }
            else if (type == 3)
            {
                needDef = 20;
                goldSet = 2500;
            }
            needDef -= (int)user.Defend;
            if (needDef > 0) { Console.WriteLine("방어력이 부실하다!"); }
            else { Console.WriteLine("방어력은 충분하다!"); }
            Thread.Sleep(2000);
            randDef += needDef;
            user.Health -= randDef;
            Console.WriteLine($"유저의 체력이 {randDef}만큼 줄어들어 현재 {user.Health}가 되었습나다...");
            Thread.Sleep(2000);
            if (user.Health <= 0)
            {
                Console.WriteLine("HP가 0이 되어서 게임 오버!");
                GameOver();
            }
            else if(needDef > (int)user.Defend && FailRandom <40)
            {
                Console.WriteLine("40%로 던전 실패");
            }
            else
            {
                ClearDungeon(user, goldSet);
            }


        }
        public void ClearDungeon(User user, int goldSet)
        {
            Random random = new Random();
            int attack = (int)user.Attack;
            int AttackPlus = random.Next(attack, attack * 2);
            goldSet += (goldSet * AttackPlus) / 100;
            Console.WriteLine($"던전 클리어\r\n축하합니다!!\r\n쉬운 던전을 클리어 하였습니다.\r\n\r\n[탐험 결과]\r\n체력 100 -> {user.Health}\r\nGold {user.Gold} G -> {user.Gold + goldSet} G \r\n\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
            user.clearMap += 1;
            user.Gold += goldSet;
            user.LevelUp(user);
            int input = int.Parse(Console.ReadLine());
            

        }
        public void GameOver() 
        {
            Console.WriteLine("프로그램을 종료합니다.");
            Environment.Exit(0); // 0은 정상 종료를 의미
        }
    }
    public class Weapon : GameManager
    {
        public string name { get; }
        public float attack { get; }
        public float defense { get; }
        public int gold { get; }
        public int type { get; }// 1이면 무기 2이면 방어구
        public bool equipItem { get; set; }
        public string story { get; }

        public Weapon(string name1, float attack1, float defense1, int gold1,int type, bool equipItem1, string story1)
        {
            this.name = name1;
            this.attack = attack1;
            this.defense = defense1;
            this.gold = gold1;
            this.type = type;
            this.equipItem = equipItem1;
            this.story = story1;
        }
        public void TransferItemToRightInventory(List<Weapon> inventory, List<Weapon> userInventory, int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= inventory.Count)
            {
                Console.WriteLine("잘못된 아이템 번호입니다.");
                return;
            }

            Weapon selectedItem = inventory[itemIndex];
                userInventory.Add(selectedItem); // 유저 인벤토리에 추가
                inventory.RemoveAt(itemIndex); // 기존 인벤토리에서 제거
                Console.WriteLine($"{selectedItem.name}을(를) 유저 인벤토리로 옮겼습니다.");

        }
        public void DrawItem(User user, List<Weapon> inventory, List<Weapon> userInventory,int type = 0) //type0은 인벤 //type1은 상점 //type2는 아이템 구매
        {
            Console.WriteLine("[아이템 목록]");
            for (int i = 0; i < inventory.Count; i++)
            {
                string attackText = inventory[i].attack != 0 ? $"공격력: {inventory[i].attack}" : "";
                string defenseText = inventory[i].defense != 0 ? $"방어력: {inventory[i].defense}" : "";        
                string priceText = inventory[i].gold != 0 ? $"가격: {inventory[i].gold}" : "";

                // 출력할 정보가 있는지 확인

                string output = $"- ";
                if (type != 1) output += $"{i + 1}. ";
                if(type == 0 && inventory[i].equipItem==true)
                {
                    output += "[E]"; 
                }
                output += $"{inventory[i].name}";
                if (!string.IsNullOrEmpty(attackText)) output += $"\t{attackText}";
                if (!string.IsNullOrEmpty(defenseText)) output += $"\t{defenseText}";
                output += $"\t{inventory[i].story}";
                if (type == 0) output += "";
                else if (!string.IsNullOrEmpty(priceText)) output += $"\t{priceText}";
                Console.WriteLine(output);
            }
            if (type == 1)
            {
                for (int i = 0; i < userInventory.Count; i++)
                {
                    string attackText = userInventory[i].attack != 0 ? $"공격력: {userInventory[i].attack}" : "";
                    string defenseText = userInventory[i].defense != 0 ? $"방어력: {userInventory[i].defense}" : "";
                    string priceText = userInventory[i].gold != 0 ? $"가격: {userInventory[i].gold}" : "";

                    // 출력할 정보가 있는지 확인

                    string output = $"- ";
                    output += $"{userInventory[i].name}";
                    if (!string.IsNullOrEmpty(attackText)) output += $"\t{attackText}";
                    if (!string.IsNullOrEmpty(defenseText)) output += $"\t{defenseText}";
                    output += $"\t{userInventory[i].story}\t 구매완료";
                    Console.WriteLine(output);
                }
            }
        }
        public void MarketInfo(User user, List<Weapon> inventory, List<Weapon> userInventory)
        {
            Console.WriteLine($"상점\r\n필요한 아이템을 얻을 수 있는 상점입니다.\r\n\r\n[보유 골드]\r\n{user.Gold}G\r\n[아이템 목록]");
            Console.WriteLine("아이템 리스트:");
            DrawItem(user, inventory, userInventory,1);//아이템 번호없애고 구매완료 있어야댐//1
            Console.WriteLine("\r\n1. 아이템 구매\r\n2. 아이템 판매\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
            int input = int.Parse(Console.ReadLine());
            if(input ==0) goBackVillage();
            else if (input == 1) BuyItem(user, inventory, userInventory);
            else if (input == 2) SellItem(user, inventory, userInventory);

        }
        public void SellItem(User user, List<Weapon> inventory, List<Weapon> userInventory)
        {
            Console.Clear();
            Console.WriteLine($"상점 - 아이템 판매\r\n필요한 아이템을 얻을 수 있는 상점입니다.\r\n\r\n[보유 골드]\r\n{user.Gold} G\r\n\r\n[아이템 목록]\r\n");
            DrawItem(user, userInventory, userInventory, 2);//구매완료 있어야댐//2
            Console.WriteLine("\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
            int input = int.Parse(Console.ReadLine());
            int itemIndex = input - 1;
           
            if (input == 0)//나가기 선택
            {
                goBackVillage();
                return;
            }
            else if (itemIndex < 0 || itemIndex >= inventory.Count)
            {
                Console.WriteLine("잘못된 입력입니다.");
                goBackVillage();

            }
            else
            {
            Weapon userItem = userInventory[itemIndex]; // 선택된 아이템 참조
                user.Gold += (userItem.gold*85)/100;
                EquipNon(user, userInventory, itemIndex);
                inventory.Add(userItem);
                userInventory.RemoveAt(itemIndex);

                Console.WriteLine($"{userItem.name}을(를) 판매했습니다.");
                goBackVillage();
            }
        }
        public void BuyItem(User user, List<Weapon> inventory, List<Weapon> userInventory)
        {
            Console.Clear();

            Console.WriteLine($"상점 - 아이템 구매\r\n필요한 아이템을 얻을 수 있는 상점입니다.\r\n\r\n[보유 골드]\r\n{user.Gold} G\r\n");
            DrawItem(user, inventory, userInventory, 2);//구매완료 있어야댐//2
            Console.WriteLine("\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
            int input = int.Parse(Console.ReadLine());
            int itemIndex = input - 1; 
           // Weapon item = inventory[itemIndex]; // 선택된 아이템 참조
            if (input == 0)//나가기 선택
            {
                goBackVillage();
                return;
            }
            else if (itemIndex < 0 || itemIndex >= inventory.Count)
            {
                Console.WriteLine("잘못된 입력입니다.");
                goBackVillage();

            }
            else
            {
                Weapon selectedItem = inventory[itemIndex];

               if (user.Gold >= selectedItem.gold)
                {
                    user.Gold -= selectedItem.gold;

                    // 구매한 아이템을 userInventory로 옮기고 inventory에서 제거
                    userInventory.Add(selectedItem);
                    inventory.RemoveAt(itemIndex);

                    Console.WriteLine($"{selectedItem.name}을(를) 구매했습니다.");
                    goBackVillage();
                }
                else
                {
                    Console.WriteLine("골드가 부족합니다.");
                    Thread.Sleep(2000);
                }
            }
        }
        public void ItemInfo(User user, List<Weapon> userInventory)
        {
            Console.WriteLine($"인벤토리\r\n보유 중인 아이템을 관리할 수 있습니다.\r\n\r\n[아이템 목록]\r\n\r\n1. 장착 관리\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
            int input = int.Parse(Console.ReadLine());

            if (input == 1)
            {
                Console.Clear();
                Console.WriteLine("인벤토리 - 장착 관리\r\n보유 중인 아이템을 관리할 수 있습니다.\r\n\r\n");
                DrawItem(user, userInventory, userInventory, 0); // 인벤토리 출력
                Console.WriteLine("\r\n0. 나가기\r\n\r\n원하시는 행동을 입력해주세요.\r\n>>");
                input = int.Parse(Console.ReadLine());
                int itemIndex = input - 1;

                if (input == 0)
                {
                    goBackVillage();
                    return;
                }
                else if (userInventory[itemIndex].equipItem == true)
                {
                    EquipNon(user, userInventory, itemIndex);
                }
                else if (itemIndex >= 0 && itemIndex < userInventory.Count)
                {
                    
                    // 선택한 아이템의 타입에 따라 현재 장착된 아이템 해제
                    for (int i = 0; i < userInventory.Count; i++)
                    {
                        // 타입이 동일한 장착 중인 아이템을 해제
                        if (userInventory[i].type == userInventory[itemIndex].type)
                        {
                            EquipNon(user, userInventory, i); // 해제

                        }
                    }
                    EquipAdd(user, userInventory, itemIndex); // 장착
                    goBackVillage();
                }
            }
            else
            {
                goBackVillage();
            }

        }
        public void EquipNon(User user, List<Weapon> userInventory, int index)
        {
            if (userInventory[index].equipItem == false) return;
                userInventory[index].equipItem = false;
                user.Attack -= userInventory[index].attack;
                user.Defend -= userInventory[index].defense;
        }
        public void EquipAdd(User user, List<Weapon> userInventory, int index)
        {
            if (userInventory[index].equipItem == true) return;
            userInventory[index].equipItem = true;
            user.Attack += userInventory[index].attack;
            user.Defend += userInventory[index].defense;
        }
    }
}
