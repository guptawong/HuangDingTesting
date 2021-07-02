using System;
using System.Collections.Generic;
using System.Linq;


namespace HuaDingTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("游戏开始");
            var players = new[] { new Player("player 1", 87373), new Player("player 2", 1297684) };
            try
            {
                var game = new Game(players);
                game.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// 代表游戏
    /// </summary>
    class Game
    {
        /// <summary>
        /// 初始化游戏
        /// </summary>
        /// <param name="players">此局游戏的玩家列表</param>
        public Game(Player[] players)
        {
            if (players == null || players.Count() != 2)
            {
                throw new Exception("必须要两位玩家才可以开始游戏");
            }
            if (HasDuplicatePlayers(players))
            {
                throw new Exception("不能有重复的玩家");
            }
            this.players = players;
        }

        /// <summary>
        /// 玩家列表
        /// </summary>
        public Player[] players { get; private set; }

        /// <summary>
        /// 当前玩家
        /// </summary>
        public Player CurrentPlayer { get; private set; }

        /// <summary>
        /// 获取牙签的行数
        /// </summary>
        public int RowCount => rows.Count();

        /// <summary>
        /// 每行的牙签数量
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        private List<int> rows = new List<int>() { 3, 5, 7 };


        /// <summary>
        /// 返回选择的行的牙签数量  
        /// </summary>
        /// <param name="rowNumber">要选择的行的行号</param>
        /// <returns></returns>
        public int getStick(int rowNumber)
        {
            if (rowNumber < 1 || rowNumber > rows.Count)
            {
                throw new Exception($"请选择1-{rows.Count}行的任一行");
            }

            return rows[rowNumber - 1];
        }


        /// <summary>
        /// 从指定行中移除指定数量的牙签
        /// </summary>
        /// <param name="rowNumber">要移除牙签的行</param>
        /// <param name="number">要移除牙签的数量</param>
        /// <returns>返回指定行剩余的牙签数量</returns>
        public int RemoveStick(int rowNumber, int number)
        {
            if (rowNumber < 1 || rowNumber > rows.Count)
            {
                throw new Exception($"请选择1-{rows.Count}行的任一行");
            }
            if (number < 0 || number > rows[rowNumber - 1])
            {
                throw new Exception($"抽出的牙签数量超出范围");
            };

            rows[rowNumber - 1] -= number;
            Console.WriteLine($"剩余{rows[rowNumber - 1]}支牙签");
            return rows[rowNumber - 1];
        }

        /// <summary>
        /// 判定游戏是否结束
        /// </summary>
        /// <returns>结束返回true,否则返回false</returns>
        public bool IsEnd()
        {
            Console.WriteLine($"共剩余{rows.Sum()}支牙签");
            return rows.Sum() == 0;
        }

        /// <summary>
        /// 更新玩家的可选择行的列表
        /// </summary>
        /// <param name="rowNumber">要移除的行的行号</param>
        public void UpdateAllowSelectingRow(int rowNumber)
        {
            foreach (var player in players)
            {
                player.RemoveAllowSelectingRows(rowNumber);
            }
        }

        /// <summary>
        /// 检查是否有重复的玩家，以姓名为唯一识别号
        /// </summary>
        /// <param name="players">要检测的玩家列表</param>
        /// <returns>true有重复姓名的玩家，false 没有重复姓名的玩家</returns>
        private bool HasDuplicatePlayers(Player[] players)
        {
            return players.GroupBy(p => p.Name).Count() != players.Count();
        }




        /// <summary>
        /// 游戏开始
        /// </summary>
        public void Start()
        {
            var round = 1;
            var end = false;
            while (!end)
            {
                Console.WriteLine($"------第{round}回合------");
                foreach (var player in players)
                {
                    CurrentPlayer = player;
                    player.play(this);
                    if (IsEnd())
                    {
                        Console.WriteLine($"{CurrentPlayer.Name}输了!!!");
                        end = true;
                        break;
                    }
                    round++;
                }
                Console.WriteLine("");
            }
        }
    }


    /// <summary>
    /// 代表一个玩家
    /// </summary>
    class Player
    {
        /// <summary>
        /// 初始化玩家
        /// </summary>
        /// <param name="name">玩家的名字</param>
        /// <param name="randomSeed">随机选择的种子数值</param>
        public Player(string name, int randomSeed)
        {
            this.Name = name;
            this.randomSeed = randomSeed;
        }

        public int randomSeed { get; private set; }
        
        /// <summary>
        /// 玩家姓名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 玩家允许选择的行号
        /// </summary>
        private List<int> allowSelectingRows = new List<int>() { 1, 2, 3 };

        /// <summary>
        /// 移除允许选择的行
        /// </summary>
        /// <param name="rowNumber">要移除的行的行号</param>
        public void RemoveAllowSelectingRows(int rowNumber)
        {
            if (allowSelectingRows.Exists(e => e == rowNumber))
                allowSelectingRows.Remove(rowNumber);
        }
        
        /// <summary>
        /// 玩家玩游戏
        /// </summary>
        /// <param name="game">游戏</param>
        public void play(Game game)
        {
            var rd = new Random(randomSeed);
            var result = SelectRow(game);
            var moveStickNumber = rd.Next(1, result.stickCount);
            Console.WriteLine($"并抽走了{moveStickNumber}");
            var remain = game.RemoveStick(result.rowNumber, moveStickNumber);
            if (remain == 0)
            {
                game.UpdateAllowSelectingRow(result.rowNumber);
            }

        }

        /// <summary>
        /// 玩家选择牙签的行
        /// </summary>
        /// <param name="game">游戏</param>
        /// <returns>返回用户选择的行号及此行的牙签数量</returns>
        private (int stickCount, int rowNumber) SelectRow(Game game)
        {
            var rd = new Random(randomSeed);
            var stickerCount = 0;
            var rowNumber = 0;

            int index = allowSelectingRows.Count > 1 ?
                rd.Next(0, allowSelectingRows.Count - 1) : 0;
            rowNumber = allowSelectingRows[index];
            Console.WriteLine($"玩家{this.Name}选择了第{rowNumber}行");
            stickerCount = game.getStick(rowNumber);
            Console.WriteLine($"有{stickerCount}支牙签");
            return (stickerCount, rowNumber);
        }
    }
}
