using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Services;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Common;
using Shin_Megami_Tensei.Utilities;
using Shin_Megami_Tensei.Validation;

namespace Shin_Megami_Tensei;

public class Game
{
        private readonly View _view;
        private readonly string _teamsFolder;
        private readonly FileLoader _fileLoader;
        private readonly TeamValidator _teamValidator;
        private readonly TurnManager _turnManager;
        private readonly DamageCalculator _damageCalculator;
        private readonly CombatService _combatService;
        private readonly SkillLoader _skillLoader;
        
        public Game(View view, string teamsFolder)
        {
            _view = view;
            _teamsFolder = teamsFolder;
            _fileLoader = new FileLoader(_teamsFolder);
            _teamValidator = new TeamValidator(_fileLoader);
            _turnManager = new TurnManager();
            _damageCalculator = new DamageCalculator();
            _skillLoader = new SkillLoader(_teamsFolder);
            _combatService = new CombatService(_turnManager, _damageCalculator, _skillLoader);
        }
        
        public void Play()
        {
            try
            {
                string[] teamFiles = _fileLoader.GetTeamFiles();
                DisplayTeamOptions(teamFiles);
        
                int selectedOption = GetUserSelection();
                if (selectedOption < 0 || selectedOption >= teamFiles.Length)
                {
                    _view.WriteLine("Selección inválida");
                    return;
                }
        
                var (player1Team, player2Team) = _fileLoader.LoadTeamsFromFile(teamFiles[selectedOption]);
        
                if (player1Team == null || player2Team == null || 
                    !_teamValidator.IsValid(player1Team) || !_teamValidator.IsValid(player2Team))
                {
                    _view.WriteLine("Archivo de equipos inválido");
                    return;
                }
        
                _combatService.InitializeCombat(player1Team, player2Team);
                RunCombat();
            }
            catch (Exception ex)
            {
                _view.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private void DisplayTeamOptions(string[] teamFiles)
        {
            _view.WriteLine("Elige un archivo para cargar los equipos");
            
            for (int i = 0; i < teamFiles.Length; i++)
            {
                _view.WriteLine($"{i}: {teamFiles[i]}");
            }
        }
        
        private int GetUserSelection()
        {
            string input = _view.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return -1;
                
            return int.Parse(input);
        }
        
        private void RunCombat()
        {
            string currentPlayerName = null;
            
            while (!_combatService.IsCombatFinished())
            {
                Team currentTeam = _combatService.CurrentTeam;
                Team opponentTeam = _combatService.OpponentTeam;
                
                if (currentPlayerName != currentTeam.PlayerName)
                {
                    currentPlayerName = currentTeam.PlayerName;
                    
                    DisplayRoundInfo(currentTeam);
                }
                
                DisplayBoardState(currentTeam, opponentTeam);
                
                DisplayTurnsInfo(_combatService.GetCurrentTurns());
                
                DisplayActionOrder(_combatService.ActionOrder);
                
                Unit activeUnit = _combatService.ActionOrder.First();
                ActionResult result = ProcessUnitTurn(activeUnit, currentTeam, opponentTeam);
                
                if (result.GameEnded)
                {
                    DisplayWinner(result.Winner);
                    break;
                }
                
                _combatService.NextTurn();
            }
            
            if (_combatService.IsCombatFinished() && _combatService.GetWinner() != null)
            {
                DisplayWinner(_combatService.GetWinner().PlayerName);
            }
        }
        
        private void DisplayRoundInfo(Team team)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"Ronda de {team.Samurai.Name} ({team.PlayerName})");
        }
        
        private void DisplayBoardState(Team currentTeam, Team opponentTeam)
        {
            Team player1Team = currentTeam.PlayerName == "J1" ? currentTeam : opponentTeam;
            Team player2Team = currentTeam.PlayerName == "J1" ? opponentTeam : currentTeam;
            
            _view.WriteLine("----------------------------------------");
    
            _view.WriteLine($"Equipo de {player1Team.Samurai.Name} ({player1Team.PlayerName})");
            DisplayTeamPositions(player1Team);
    
            _view.WriteLine($"Equipo de {player2Team.Samurai.Name} ({player2Team.PlayerName})");
            DisplayTeamPositions(player2Team);
            
        }
        
        private void DisplayTeamPositions(Team team)
        {
            char position = 'A';
            foreach (var unit in team.BoardPositions)
            {
                if (unit != null)
                {
                    if (unit.IsAlive || unit is Samurai)
                    {
                        _view.WriteLine($"{position}-{unit.Name} HP:{unit.CurrentHP}/{unit.MaxHP} MP:{unit.CurrentMP}/{unit.MaxMP}");
                    }
                    else
                    {
                        _view.WriteLine($"{position}-");
                    }
                }
                else
                {
                    _view.WriteLine($"{position}-");
                }
                position++;
            }
        }
        
        private void DisplayTurnsInfo(TurnInfo turnInfo)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"Full Turns: {turnInfo.FullTurns}");
            _view.WriteLine($"Blinking Turns: {turnInfo.BlinkingTurns}");
        }
        
        private void DisplayActionOrder(List<Unit> units)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine("Orden:");
            for (int i = 0; i < units.Count; i++)
            {
                _view.WriteLine($"{i+1}-{units[i].Name}");
            }
        }
        
        private void DisplayActionResult(ActionResult result)
        {
            if (!string.IsNullOrEmpty(result.Message))
            {
                _view.WriteLine(result.Message);
            }
    
            foreach (var message in result.Messages)
            {
                _view.WriteLine(message);
            }
    
            if (result.TurnConsumptionResult != null)
            {
                DisplayTurnConsumption(result.TurnConsumptionResult);
            }
        }

        private void DisplayTurnConsumption(TurnConsumptionResult turnResult)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"Se han consumido {turnResult.FullTurnsConsumed} Full Turn(s) y {turnResult.BlinkingTurnsConsumed} Blinking Turn(s)");
            _view.WriteLine($"Se han obtenido {turnResult.BlinkingTurnsGained} Blinking Turn(s)");
        }
        
        private ActionResult ProcessUnitTurn(Unit unit, Team currentTeam, Team opponentTeam)
        {
            bool actionSelected = false;
            ActionResult result = null;
            
            while (!actionSelected)
            {
                _view.WriteLine("----------------------------------------");
                _view.WriteLine($"Seleccione una acción para {unit.Name}");
                
                List<string> availableActions = unit.GetAvailableActions();
                for (int i = 0; i < availableActions.Count; i++)
                {
                    _view.WriteLine($"{i + 1}: {availableActions[i]}");
                }
                
                int actionIndex = int.Parse(_view.ReadLine()) - 1;
                
                if (actionIndex < 0 || actionIndex >= availableActions.Count)
                {
                    result = new ActionResult { Success = false, Message = "Acción inválida" };
                    continue;
                }
                
                string selectedAction = availableActions[actionIndex];
                
                switch (selectedAction)
                {
                    case "Atacar":
                        result = HandleAttackAction(unit, opponentTeam);
                        break;
                    case "Disparar":
                        if (unit is Samurai)
                            result = HandleShootAction(unit, opponentTeam);
                        else
                            result = new ActionResult { Success = false, Message = "Solo los samurai pueden disparar" };
                        break;
                    case "Usar Habilidad":
                        result = HandleSkillAction(unit, currentTeam, opponentTeam);
                        break;
                    case "Invocar":
                        result = HandleInvokeAction(unit, currentTeam);
                        break;
                    case "Pasar Turno":
                        result = HandlePassTurnAction();
                        break;
                    case "Rendirse":
                        result = ExecuteSurrender(currentTeam.PlayerName, currentTeam, opponentTeam);
                        break;
                    default:
                        result = new ActionResult { Success = false, Message = "Opción no implementada" };
                        break;
                }
                
                if (result != null && !result.Cancelled)
                {
                    actionSelected = true;
                }
            }
            
            return result;
        }
        
        private ActionResult HandleAttackAction(Unit attacker, Team targetTeam)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"Seleccione un objetivo para {attacker.Name}");

            List<Unit> possibleTargets = targetTeam.BoardPositions
                .Where(u => u != null && u.IsAlive)
                .ToList();
            
            for (int i = 0; i < possibleTargets.Count; i++)
            {
                Unit enemy = possibleTargets[i];
                _view.WriteLine($"{i + 1}-{enemy.Name} HP:{enemy.CurrentHP}/{enemy.MaxHP} MP:{enemy.CurrentMP}/{enemy.MaxMP}");
            }

            _view.WriteLine($"{possibleTargets.Count + 1}-Cancelar");

            int selectedIndex = int.Parse(_view.ReadLine()) - 1;

            if (selectedIndex == possibleTargets.Count)
            {
                return new ActionResult { Cancelled = true };
            }

            if (selectedIndex < 0 || selectedIndex >= possibleTargets.Count)
            {
                return new ActionResult { Success = false, Message = "Objetivo inválido" };
            }

            _view.WriteLine("----------------------------------------");
            Unit selectedEnemy = possibleTargets[selectedIndex];
            ActionResult result = _combatService.ExecuteAttack(attacker, selectedEnemy);
            foreach (var message in result.Messages)
            {
                _view.WriteLine(message);
            }
            DisplayTurnConsumption(result.TurnConsumptionResult);

            return result;
        }

        private ActionResult HandleShootAction(Unit attacker, Team targetTeam)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"Seleccione un objetivo para {attacker.Name}");

            List<Unit> possibleTargets = targetTeam.BoardPositions
                .Where(u => u != null && u.IsAlive)
                .ToList();
            
            for (int i = 0; i < possibleTargets.Count; i++)
            {
                Unit enemy = possibleTargets[i];
                _view.WriteLine($"{i + 1}-{enemy.Name} HP:{enemy.CurrentHP}/{enemy.MaxHP} MP:{enemy.CurrentMP}/{enemy.MaxMP}");
            }

            _view.WriteLine($"{possibleTargets.Count + 1}-Cancelar");

            int selectedIndex = int.Parse(_view.ReadLine()) - 1;

            if (selectedIndex == possibleTargets.Count)
            {
                return new ActionResult { Cancelled = true };
            }

            if (selectedIndex < 0 || selectedIndex >= possibleTargets.Count)
            {
                return new ActionResult { Success = false, Message = "Objetivo inválido" };
            }

            _view.WriteLine("----------------------------------------");
            Unit selectedEnemy = possibleTargets[selectedIndex];
            ActionResult result = _combatService.ExecuteShoot(attacker, selectedEnemy);

            foreach (var message in result.Messages)
            {
                _view.WriteLine(message);
            }
            
            DisplayTurnConsumption(result.TurnConsumptionResult);

            return result;
        }
        private ActionResult HandleSkillAction(Unit unit, Team currentTeam, Team opponentTeam)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"Seleccione una habilidad para que {unit.Name} use");

            List<Skill> availableSkills = new List<Skill>();
    
            if (unit is Samurai samurai)
            {
                availableSkills = _skillLoader.GetSkills(samurai.SkillNames);
            }
            else if (unit is Monster monster)
            {
                availableSkills = _skillLoader.GetSkills(monster.SkillNames);
            }
    
            List<Skill> usableSkills = availableSkills
                .Where(s => unit.CurrentMP >= s.Cost)
                .ToList();

            for (int i = 0; i < usableSkills.Count; i++)
            {
                _view.WriteLine($"{i + 1}-{usableSkills[i].Name} MP:{usableSkills[i].Cost}");
            }
    
            _view.WriteLine($"{usableSkills.Count + 1}-Cancelar");
    
            int skillIndex = int.Parse(_view.ReadLine()) - 1;
    
            if (skillIndex == usableSkills.Count || skillIndex < 0 || skillIndex >= usableSkills.Count)
            {
                return new ActionResult { Cancelled = true };
            }
    
            Skill selectedSkill = usableSkills[skillIndex];

            if (selectedSkill.Target == "Single")
            {
                if (selectedSkill is OffensiveSkill)
                {
                    return HandleOffensiveSkillTarget(unit, opponentTeam, selectedSkill);
                }
            }
            else if (selectedSkill.Target == "Ally")
            {
                if (selectedSkill is HealSkill || selectedSkill.Name == "Invitation" || selectedSkill.Name == "Sabbatma")
                {
                    return HandleSupportSkillTarget(unit, currentTeam, selectedSkill);
                }
            }
    
            return new ActionResult { Success = false, Message = "Tipo de habilidad no soportado" };
        }

        private ActionResult HandleOffensiveSkillTarget(Unit attacker, Team targetTeam, Skill skill)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"Seleccione un objetivo para {attacker.Name}");

            List<Unit> possibleTargets = targetTeam.BoardPositions
                .Where(u => u != null && u.IsAlive)
                .ToList();
            
            for (int i = 0; i < possibleTargets.Count; i++)
            {
                Unit enemy = possibleTargets[i];
                _view.WriteLine($"{i + 1}-{enemy.Name} HP:{enemy.CurrentHP}/{enemy.MaxHP} MP:{enemy.CurrentMP}/{enemy.MaxMP}");
            }

            _view.WriteLine($"{possibleTargets.Count + 1}-Cancelar");

            int selectedIndex = int.Parse(_view.ReadLine()) - 1;

            if (selectedIndex == possibleTargets.Count)
            {
                return new ActionResult { Cancelled = true };
            }

            if (selectedIndex < 0 || selectedIndex >= possibleTargets.Count)
            {
                return new ActionResult { Success = false, Message = "Objetivo inválido" };
            }

            _view.WriteLine("----------------------------------------");

            Unit selectedEnemy = possibleTargets[selectedIndex];
            ActionResult result = _combatService.ExecuteSkill(attacker, selectedEnemy, skill);

            foreach (var message in result.Messages)
            {
                _view.WriteLine(message);
            }

            DisplayTurnConsumption(result.TurnConsumptionResult);

            return result;
        }

        private ActionResult HandleSupportSkillTarget(Unit user, Team team, Skill skill)
        {
            _view.WriteLine("----------------------------------------");
            
            List<Unit> possibleTargets = new List<Unit>();
            
            if (skill.Name == "Invitation" || skill.Name == "Sabbatma")
            {
                if (skill.Name == "Invitation")
                {
                    possibleTargets = team.Reserve.Cast<Unit>().ToList();
                }
                else
                {
                    possibleTargets = team.Reserve
                        .Where(m => m.IsAlive)
                        .Cast<Unit>()
                        .ToList();
                }
                
                _view.WriteLine($"Seleccione un monstruo para invocar");
            }
            else if (skill is HealSkill healSkill)
            {
                if (healSkill.Name.Contains("Recarm") || healSkill.Name.Contains("Samarecarm"))
                {
                    var deadUnitsOnBoard = team.BoardPositions
                        .Where(u => u != null && !u.IsAlive)
                        .ToList();
                    var deadUnitsInReserve = team.Reserve
                        .Where(u => !u.IsAlive)
                        .Cast<Unit>()
                        .ToList();
                    
                    possibleTargets = deadUnitsOnBoard.Concat(deadUnitsInReserve).ToList();
                }
                else
                {
                    possibleTargets = team.BoardPositions
                        .Where(u => u != null && u.IsAlive)
                        .ToList();
                }
                
                _view.WriteLine($"Seleccione un objetivo para {user.Name}");
            }
            
            for (int i = 0; i < possibleTargets.Count; i++)
            {
                Unit target = possibleTargets[i];
                _view.WriteLine($"{i + 1}-{target.Name} HP:{target.CurrentHP}/{target.MaxHP} MP:{target.CurrentMP}/{target.MaxMP}");
            }
            
            _view.WriteLine($"{possibleTargets.Count + 1}-Cancelar");
            
            int selectedIndex = int.Parse(_view.ReadLine()) - 1;
            
            if (selectedIndex == possibleTargets.Count)
            {
                return new ActionResult { Cancelled = true };
            }
            
            if (selectedIndex < 0 || selectedIndex >= possibleTargets.Count)
            {
                return new ActionResult { Success = false, Message = "Objetivo inválido" };
            }
            
            Unit selectedTarget = possibleTargets[selectedIndex];
            
            if (skill.Name == "Invitation" || skill.Name == "Sabbatma")
            {
                return HandleInvocationPosition(user, selectedTarget as Monster, skill);
            }
            
            _view.WriteLine("----------------------------------------");
            
            ActionResult result = _combatService.ExecuteSkill(user, selectedTarget, skill);
            
            foreach (var message in result.Messages)
            {
                _view.WriteLine(message);
            }
            
            DisplayTurnConsumption(result.TurnConsumptionResult);
            
            return result;
        }

        private ActionResult HandleInvocationPosition(Unit summoner, Monster monster, Skill skill)
        {
            Team team = _combatService.CurrentTeam;
            
            if (skill != null && (skill.Name == "Sabbatma" || skill.Name == "Invitation"))
            {
                _view.WriteLine("----------------------------------------");
                _view.WriteLine($"Seleccione una posición para invocar");
                
                for (int i = 1; i < team.BoardPositions.Length; i++)
                {
                    if (team.BoardPositions[i] == null)
                    {
                        _view.WriteLine($"{i}-Vacío (Puesto {i+1})");
                    }
                    else
                    {
                        Unit unit = team.BoardPositions[i];
                        _view.WriteLine($"{i}-{unit.Name} HP:{unit.CurrentHP}/{unit.MaxHP} MP:{unit.CurrentMP}/{unit.MaxMP} (Puesto {i+1})");
                    }
                }
                
                _view.WriteLine($"{team.BoardPositions.Length}-Cancelar");
                
                int positionIndex = int.Parse(_view.ReadLine()) - 1;
                
                if (positionIndex == team.BoardPositions.Length - 1)
                {
                    return new ActionResult { Cancelled = true };
                }
                
                if (positionIndex < 0 || positionIndex >= team.BoardPositions.Length - 1)
                {
                    return new ActionResult { Success = false, Message = "Posición inválida" };
                }
                
                int boardPosition = positionIndex + 1;
                
                _view.WriteLine("----------------------------------------");
                
                ActionResult result = _combatService.ExecuteSkill(summoner, monster, skill);

                foreach (var message in result.Messages)
                {
                    _view.WriteLine(message);
                }

                if (result.SpecialAction == "Summon")
                {
                    if (team.BoardPositions[boardPosition] == null)
                    {
                        team.BoardPositions[boardPosition] = monster;
                        team.Reserve.Remove(monster);
                        
                        _combatService.ActionOrder.Add(monster);
                    }
                    else
                    {
                        Monster existingMonster = team.BoardPositions[boardPosition] as Monster;
                        if (existingMonster != null)
                        {
                            team.BoardPositions[boardPosition] = monster;
                            team.Reserve.Remove(monster);
                            team.Reserve.Add(existingMonster);
                            
                            var actionOrder = _combatService.ActionOrder;
                            int orderIndex = actionOrder.IndexOf(existingMonster);
                            if (orderIndex >= 0)
                            {
                                actionOrder[orderIndex] = monster;
                            }
                        }
                    }
                }
                
                DisplayTurnConsumption(result.TurnConsumptionResult);
                return result;
            }
            
            if (summoner is Monster)
            {
                _view.WriteLine("----------------------------------------");
                
                ActionResult result = _combatService.ExecuteSkill(summoner, monster, skill);

                foreach (var message in result.Messages)
                {
                    _view.WriteLine(message);
                }

                if (result.SpecialAction == "Summon")
                {
                    int summonerPosition = Array.IndexOf(team.BoardPositions, summoner);
                    if (summonerPosition >= 0)
                    {
                        team.BoardPositions[summonerPosition] = monster;
                        team.Reserve.Remove(monster);
                        team.Reserve.Add(summoner as Monster);
                        
                        var actionOrder = _combatService.ActionOrder;
                        int orderIndex = actionOrder.IndexOf(summoner);
                        if (orderIndex >= 0)
                        {
                            actionOrder[orderIndex] = monster;
                        }
                    }
                }
                
                DisplayTurnConsumption(result.TurnConsumptionResult);
                return result;
            }
            else
            {
                // This code path should not be reached since skill-based invocation is handled above
                return new ActionResult { Success = false, Message = "Invalid invocation" };
            }
        }

        private ActionResult HandleInvokeAction(Unit summoner, Team team)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"Seleccione un monstruo para invocar");

            List<Monster> monstersInReserve = team.Reserve
                .Where(m => m.IsAlive)
                .ToList();
            
            if (monstersInReserve.Count == 0)
            {
                _view.WriteLine("No hay monstruos disponibles en reserva");
                _view.WriteLine("1-Cancelar");
                _view.ReadLine();
                return new ActionResult { Cancelled = true };
            }
            
            for (int i = 0; i < monstersInReserve.Count; i++)
            {
                Monster monster = monstersInReserve[i];
                _view.WriteLine($"{i + 1}-{monster.Name} HP:{monster.CurrentHP}/{monster.MaxHP} MP:{monster.CurrentMP}/{monster.MaxMP}");
            }
            
            _view.WriteLine($"{monstersInReserve.Count + 1}-Cancelar");
            
            int monsterIndex = int.Parse(_view.ReadLine()) - 1;
            
            if (monsterIndex == monstersInReserve.Count)
            {
                return new ActionResult { Cancelled = true };
            }
            
            if (monsterIndex < 0 || monsterIndex >= monstersInReserve.Count)
            {
                return new ActionResult { Success = false, Message = "Monstruo inválido" };
            }
            
            Monster selectedMonster = monstersInReserve[monsterIndex];
            
            if (summoner is Monster)
            {
                _view.WriteLine("----------------------------------------");
                
                ActionResult result = _combatService.ExecuteInvoke(summoner, selectedMonster);
                
                _view.WriteLine($"{selectedMonster.Name} ha sido invocado");
                DisplayTurnConsumption(result.TurnConsumptionResult);
                
                return result;
            }
            else
            {
                _view.WriteLine("----------------------------------------");
                _view.WriteLine($"Seleccione una posición para invocar");
                
                for (int i = 1; i < team.BoardPositions.Length; i++)
                {
                    if (team.BoardPositions[i] == null)
                    {
                        _view.WriteLine($"{i}-Vacío (Puesto {i+1})");
                    }
                    else
                    {
                        Unit unit = team.BoardPositions[i];
                        _view.WriteLine($"{i}-{unit.Name} HP:{unit.CurrentHP}/{unit.MaxHP} MP:{unit.CurrentMP}/{unit.MaxMP} (Puesto {i+1})");
                    }
                }
                
                _view.WriteLine($"{team.BoardPositions.Length}-Cancelar");
                
                int positionIndex = int.Parse(_view.ReadLine()) - 1;
                
                if (positionIndex == team.BoardPositions.Length - 1)
                {
                    return new ActionResult { Cancelled = true };
                }
                
                if (positionIndex < 0 || positionIndex >= team.BoardPositions.Length - 1)
                {
                    return new ActionResult { Success = false, Message = "Posición inválida" };
                }
                
                int boardPosition = positionIndex + 1;
                
                _view.WriteLine("----------------------------------------");
                
                ActionResult result = _combatService.ExecuteInvoke(summoner, selectedMonster, boardPosition);
                
                _view.WriteLine($"{selectedMonster.Name} ha sido invocado");
                DisplayTurnConsumption(result.TurnConsumptionResult);
                
                return result;
            }
        }

        private ActionResult HandlePassTurnAction()
        {
            ActionResult result = _combatService.ExecutePassTurn();
            
            // Mostrar consumo de turnos
            DisplayTurnConsumption(result.TurnConsumptionResult);
            
            return result;
        }
        private ActionResult ExecuteSurrender(string playerName, Team currentTeam, Team opponentTeam)
        {
            _view.WriteLine("----------------------------------------");
            _view.WriteLine($"{currentTeam.Samurai.Name} ({playerName}) se rinde");
    
            return new ActionResult { 
                Success = true, 
                GameEnded = true, 
                Winner = opponentTeam.PlayerName
            };
        }
        
        private void DisplayWinner(string winnerName)
        {
            _view.WriteLine("----------------------------------------");
    
            Team winnerTeam = _combatService.CurrentTeam.PlayerName == winnerName ? 
                _combatService.CurrentTeam : _combatService.OpponentTeam;
    
            _view.WriteLine($"Ganador: {winnerTeam.Samurai.Name} ({winnerTeam.PlayerName})");
        }
    }