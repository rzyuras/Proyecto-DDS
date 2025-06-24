using System.Text.Json;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Models.JSON;

namespace Shin_Megami_Tensei.Utilities;

public class FileLoader
    {
        private readonly string _dataFolder;
        private readonly string _teamsFolder;
        private Dictionary<string, SamuraiData> _samuraiData;
        private List<string> _validSkillNames;
        private Dictionary<string, MonsterData> _monsterData;

        public FileLoader(string teamsFolder)
        {
            _teamsFolder = teamsFolder;
            _dataFolder = Path.GetDirectoryName(teamsFolder) ?? teamsFolder;
            
            LoadSamuraiData();
            LoadSkillNames();
            LoadMonsterData();
        }

        private void LoadSamuraiData()
        {
            try
            {
                string jsonPath = Path.Combine(_dataFolder, "samurai.json");
                string json = File.ReadAllText(jsonPath);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var samurais = JsonSerializer.Deserialize<List<SamuraiData>>(json, options);
                _samuraiData = samurais.ToDictionary(s => s.Name, s => s);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading samurai data: {ex.Message}");
                throw;
            }
        }

        private void LoadSkillNames()
        {
            try
            {
                string jsonPath = Path.Combine(_dataFolder, "skills.json");
                string json = File.ReadAllText(jsonPath);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var skills = JsonSerializer.Deserialize<List<SkillData>>(json, options);
                _validSkillNames = skills.Select(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading skill data: {ex.Message}");
                throw;
            }
        }

        private void LoadMonsterData()
        {
            try
            {
                string jsonPath = Path.Combine(_dataFolder, "monsters.json");
                string json = File.ReadAllText(jsonPath);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var monsters = JsonSerializer.Deserialize<List<MonsterData>>(json, options);
                _monsterData = monsters.ToDictionary(m => m.Name, m => m);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading monster data: {ex.Message}");
                throw;
            }
        }

        public string[] GetTeamFiles()
        {
            return Directory.GetFiles(_teamsFolder, "*.txt")
                .Select(Path.GetFileName)
                .OrderBy(f => f)
                .ToArray();
        }

        public (Team player1Team, Team player2Team) LoadTeamsFromFile(string filename)
        {
            try
            {
                string filePath = Path.Combine(_teamsFolder, filename);
                string[] lines = File.ReadAllLines(filePath);

                Team player1Team = new Team { PlayerName = "J1" };
                Team player2Team = new Team { PlayerName = "J2" };

                bool isPlayer1Section = false;
                bool isPlayer2Section = false;
                
                bool player1HasSamurai = false;
                bool player2HasSamurai = false;

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    
                    if (trimmedLine == "Player 1 Team")
                    {
                        isPlayer1Section = true;
                        isPlayer2Section = false;
                        continue;
                    }
                    
                    if (trimmedLine == "Player 2 Team")
                    {
                        isPlayer1Section = false;
                        isPlayer2Section = true;
                        continue;
                    }
                    
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                        continue;
                    
                    if (trimmedLine.Contains("[Samurai]"))
                    {
                        var (samuraiName, skillNames) = ParseSamuraiLine(trimmedLine);
                        
                        if (_samuraiData.TryGetValue(samuraiName, out SamuraiData samuraiData))
                        {
                            Samurai samurai = CreateSamuraiFromData(samuraiData);
                            samurai.SkillNames = skillNames;
                            
                            if (isPlayer1Section)
                            {
                                if (player1HasSamurai)
                                {
                                    return (null, null);
                                }
                                
                                player1Team.Samurai = samurai;
                                player1HasSamurai = true;
                            }
                            else if (isPlayer2Section)
                            {
                                if (player2HasSamurai)
                                {
                                    return (null, null);
                                }
                                
                                player2Team.Samurai = samurai;
                                player2HasSamurai = true;
                            }
                        }
                    }
                    else if (!trimmedLine.StartsWith("Player"))
                    {
                        string monsterName = trimmedLine;
                        
                        if (_monsterData.TryGetValue(monsterName, out MonsterData monsterData))
                        {
                            Monster monster = CreateMonsterFromData(monsterData);
                            
                            if (isPlayer1Section)
                                player1Team.Monsters.Add(monster);
                            else if (isPlayer2Section)
                                player2Team.Monsters.Add(monster);
                        }
                    }
                }
                
                if (player1Team.Samurai != null && player2Team.Samurai != null)
                {
                    player1Team.InitializeBoard();
                    player2Team.InitializeBoard();
                    return (player1Team, player2Team);
                }
                else
                {
                    return (null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading teams: {ex.Message}");
                throw;
            }
        }

        private (string name, List<string> skills) ParseSamuraiLine(string line)
        {
            string[] parts = line.Split('(');
            string name = parts[0].Replace("[Samurai]", "").Trim();
            List<string> skills = new List<string>();
            
            if (parts.Length > 1)
            {
                string skillsText = parts[1].TrimEnd(')');
                skills = skillsText.Split(',').Select(s => s.Trim()).ToList();
            }
            
            return (name, skills);
        }

        private Samurai CreateSamuraiFromData(SamuraiData data)
        {
            return new Samurai
            {
                Name = data.Name,
                MaxHP = data.Stats.HP,
                CurrentHP = data.Stats.HP,
                MaxMP = data.Stats.MP,
                CurrentMP = data.Stats.MP,
                Str = data.Stats.Str,
                Skl = data.Stats.Skl,
                Mag = data.Stats.Mag,
                Spd = data.Stats.Spd,
                Lck = data.Stats.Lck,
                Affinity = data.Affinity,
                SkillNames = new List<string>()
            };
        }

        private Monster CreateMonsterFromData(MonsterData data)
        {
            Monster monster = new Monster
            {
                Name = data.Name,
                MaxHP = data.Stats.HP,
                CurrentHP = data.Stats.HP,
                MaxMP = data.Stats.MP,
                CurrentMP = data.Stats.MP,
                Str = data.Stats.Str,
                Skl = data.Stats.Skl,
                Mag = data.Stats.Mag,
                Spd = data.Stats.Spd,
                Lck = data.Stats.Lck,
                Affinity = data.Affinity,
                SkillNames = data.Skills
            };
            
            return monster;
        }
        
        public bool IsValidSkillName(string skillName)
        {
            return _validSkillNames.Contains(skillName);
        }
    }