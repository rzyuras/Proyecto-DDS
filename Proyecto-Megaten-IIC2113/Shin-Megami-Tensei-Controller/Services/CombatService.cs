namespace Shin_Megami_Tensei.Services;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Common;
using Shin_Megami_Tensei.Utilities;

public class CombatService
    {
        private readonly TurnManager _turnManager;
        private readonly DamageCalculator _damageCalculator;
        private readonly SkillLoader _skillLoader;
        
        public Team CurrentTeam { get; private set; }
        public Team OpponentTeam { get; private set; }
        public List<Unit> ActionOrder { get; private set; } = new List<Unit>();
        
        public CombatService(TurnManager turnManager, DamageCalculator damageCalculator, SkillLoader skillLoader)
        {
            _turnManager = turnManager;
            _damageCalculator = damageCalculator;
            _skillLoader = skillLoader;
        }
        
        public void InitializeCombat(Team player1Team, Team player2Team)
        {
            player1Team.InitializeBoard();
            player2Team.InitializeBoard();
            
            CurrentTeam = player1Team;
            OpponentTeam = player2Team;
            
            _turnManager.ResetTurns(CurrentTeam.CountAliveUnits());
            CalculateActionOrder();
        }
        
        private void CalculateActionOrder()
        {
            ActionOrder = CurrentTeam.BoardPositions
                .Where(u => u != null && u.IsAlive)
                .OrderByDescending(u => u.Spd)
                .ThenBy(u => Array.IndexOf(CurrentTeam.BoardPositions, u))
                .ToList();
        }
        
        public ActionResult ExecuteAttack(Unit attacker, Unit target)
        {
            AffinityResult affinityResult = _damageCalculator.GetAffinityResult(target, GameConstants.PhysElement);
            int damage = _damageCalculator.CalculateAttackDamage(attacker, target);
            
            List<string> messages = new List<string>();
            messages.Add($"{attacker.Name} ataca a {target.Name}");
            
            if (affinityResult.Type == GameConstants.WeakAffinity)
            {
                messages.Add($"{target.Name} es débil contra el ataque de {attacker.Name}");
            }
            else if (affinityResult.Type == GameConstants.ResistAffinity)
            {
                messages.Add($"{target.Name} es resistente el ataque de {attacker.Name}");
            }
            else if (affinityResult.Type == GameConstants.NullAffinity)
            {
                messages.Add($"{target.Name} bloquea el ataque de {attacker.Name}");
                damage = 0;
            }
            else if (affinityResult.Type == GameConstants.RepelAffinity)
            {
                messages.Add($"{target.Name} devuelve {Math.Abs(damage)} daño a {attacker.Name}");
                attacker.TakeDamage(Math.Abs(damage));
                HandleUnitDeath(attacker, CurrentTeam);
                messages.Add($"{attacker.Name} termina con HP:{attacker.CurrentHP}/{attacker.MaxHP}");
                
                TurnConsumptionResult turnResult = _turnManager.ConsumeTurnBasedOnAffinity(affinityResult.Type);
                return new ActionResult 
                { 
                    Success = true,
                    Messages = messages,
                    TurnConsumptionResult = turnResult,
                    AffinityType = affinityResult.Type 
                };
            }
            else if (affinityResult.Type == GameConstants.DrainAffinity)
            {
                messages.Add($"{target.Name} absorbe {Math.Abs(damage)} daño");
                target.CurrentHP = Math.Min(target.MaxHP, target.CurrentHP + Math.Abs(damage));
                messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
                
                TurnConsumptionResult turnResult = _turnManager.ConsumeTurnBasedOnAffinity(affinityResult.Type);
                return new ActionResult 
                { 
                    Success = true,
                    Messages = messages,
                    TurnConsumptionResult = turnResult,
                    AffinityType = affinityResult.Type 
                };
            }
            
            if (affinityResult.Type != GameConstants.NullAffinity && 
                affinityResult.Type != GameConstants.RepelAffinity && 
                affinityResult.Type != GameConstants.DrainAffinity)
            {
                target.TakeDamage(damage);
                HandleUnitDeath(target, OpponentTeam);
                messages.Add($"{target.Name} recibe {damage} de daño");
            }
            
            messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
            
            TurnConsumptionResult turnConsumption = _turnManager.ConsumeTurnBasedOnAffinity(affinityResult.Type);

            return new ActionResult 
            { 
                Success = true,
                Messages = messages,
                DamageDealt = damage,
                TurnConsumptionResult = turnConsumption,
                AffinityType = affinityResult.Type 
            };
        }
        public ActionResult ExecuteShoot(Unit attacker, Unit target)
        {
            AffinityResult affinityResult = _damageCalculator.GetAffinityResult(target, GameConstants.GunElement);
            int damage = _damageCalculator.CalculateShootDamage(attacker, target);
            
            List<string> messages = new List<string>();
            messages.Add($"{attacker.Name} dispara a {target.Name}");
            
            if (affinityResult.Type == GameConstants.WeakAffinity)
            {
                messages.Add($"{target.Name} es débil contra el ataque de {attacker.Name}");
            }
            else if (affinityResult.Type == GameConstants.ResistAffinity)
            {
                messages.Add($"{target.Name} es resistente el ataque de {attacker.Name}");
            }
            else if (affinityResult.Type == GameConstants.NullAffinity)
            {
                messages.Add($"{target.Name} bloquea el ataque de {attacker.Name}");
                damage = 0;
            }
            else if (affinityResult.Type == GameConstants.RepelAffinity)
            {
                messages.Add($"{target.Name} devuelve {Math.Abs(damage)} daño a {attacker.Name}");
                attacker.TakeDamage(Math.Abs(damage));
                HandleUnitDeath(attacker, CurrentTeam);
                messages.Add($"{attacker.Name} termina con HP:{attacker.CurrentHP}/{attacker.MaxHP}");
                
                TurnConsumptionResult turnResult = _turnManager.ConsumeTurnBasedOnAffinity(affinityResult.Type);
                return new ActionResult 
                { 
                    Success = true,
                    Messages = messages,
                    TurnConsumptionResult = turnResult,
                    AffinityType = affinityResult.Type 
                };
            }
            else if (affinityResult.Type == GameConstants.DrainAffinity)
            {
                messages.Add($"{target.Name} absorbe {Math.Abs(damage)} daño");
                target.CurrentHP = Math.Min(target.MaxHP, target.CurrentHP + Math.Abs(damage));
                messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
                
                TurnConsumptionResult turnResult = _turnManager.ConsumeTurnBasedOnAffinity(affinityResult.Type);
                return new ActionResult 
                { 
                    Success = true,
                    Messages = messages,
                    TurnConsumptionResult = turnResult,
                    AffinityType = affinityResult.Type 
                };
            }

            if (affinityResult.Type != GameConstants.NullAffinity)
            {
                target.TakeDamage(damage);
                HandleUnitDeath(target, OpponentTeam);
                messages.Add($"{target.Name} recibe {damage} de daño");
            }
            
            messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
            
            TurnConsumptionResult turnConsumption = _turnManager.ConsumeTurnBasedOnAffinity(affinityResult.Type);

            return new ActionResult 
            { 
                Success = true,
                Messages = messages,
                DamageDealt = damage,
                TurnConsumptionResult = turnConsumption,
                AffinityType = affinityResult.Type
            };
        }
        
        public ActionResult ExecuteSurrender(string playerName)
        {
            return new ActionResult 
            { 
                Success = true, 
                Message = string.Format(GameMessages.SurrenderFormat, playerName),
                GameEnded = true,
                Winner = OpponentTeam.PlayerName
            };
        }
        
        public ActionResult ExecuteSkill(Unit user, Unit target, Skill skill)
        {
            user.CurrentMP -= skill.Cost;
            ActionResult result = skill.Execute(user, target, CurrentTeam, OpponentTeam, _turnManager, _damageCalculator);
    
            if (result.Success && skill is OffensiveSkill)
            {
                HandleUnitDeath(user, CurrentTeam);
                HandleUnitDeath(target, OpponentTeam);
            }
    
            return result;
        }
    
        public ActionResult ExecutePassTurn()
        {
            var result = new ActionResult { Success = true };
            result.TurnConsumptionResult = _turnManager.ConsumeInvokeOrPassTurn();
            return result;
        }
        
        public ActionResult ExecuteInvoke(Unit summoner, Monster monster, int position = -1)
        {
            var result = new ActionResult { Success = true };
            
            if (summoner is Samurai)
            {
                if (position < 0 || position >= CurrentTeam.BoardPositions.Length || position == GameConstants.InitialPositionForSamurai)
                {
                    result.Success = false;
                    result.Message = "Posición inválida para invocar";
                    return result;
                }
                
                if (CurrentTeam.BoardPositions[position] == null)
                {
                    CurrentTeam.BoardPositions[position] = monster;
                    CurrentTeam.Reserve.Remove(monster);
                }
                else
                {
                    Monster existingMonster = CurrentTeam.BoardPositions[position] as Monster;
                    if (existingMonster == null)
                    {
                        result.Success = false;
                        result.Message = "No se puede reemplazar a un samurai";
                        return result;
                    }
                    
                    CurrentTeam.BoardPositions[position] = monster;
                    CurrentTeam.Reserve.Remove(monster);
                    CurrentTeam.Reserve.Add(existingMonster);
                }
                
                result.Messages.Add($"{monster.Name} ha sido invocado");
            }
            else if (summoner is Monster)
            {
                int summonerPosition = Array.IndexOf(CurrentTeam.BoardPositions, summoner);
                if (summonerPosition < 0)
                {
                    result.Success = false;
                    result.Message = "No se encontró la posición del invocador";
                    return result;
                }
                
                CurrentTeam.BoardPositions[summonerPosition] = monster;
                CurrentTeam.Reserve.Remove(monster);
                CurrentTeam.Reserve.Add(summoner as Monster);
                
                result.Messages.Add($"{monster.Name} ha sido invocado");
            }
            
            RecalculateActionOrder();
            result.TurnConsumptionResult = _turnManager.ConsumeInvokeOrPassTurn();
            
            return result;
        }
        private void RecalculateActionOrder()
        {
            var aliveUnits = CurrentTeam.BoardPositions
                .Where(u => u != null && u.IsAlive)
                .ToList();
                
            if (!aliveUnits.Contains(ActionOrder.FirstOrDefault()))
            {
                ActionOrder = aliveUnits
                    .OrderByDescending(u => u.Spd)
                    .ToList();
                return;
            }
            
            var newUnits = aliveUnits
                .Where(u => !ActionOrder.Contains(u))
                .ToList();
                
            foreach (var newUnit in newUnits)
            {
                ActionOrder.Add(newUnit);
            }
            
            ActionOrder = ActionOrder
                .Where(u => aliveUnits.Contains(u))
                .ToList();
        }
        
        public List<ActionResult> ExecuteMultiTargetSkill(Unit user, List<Unit> targets, Skill skill)
        {
            List<ActionResult> results = new List<ActionResult>();
            user.CurrentMP -= skill.Cost;
            
            foreach (var target in targets)
            {
                var result = skill.Execute(user, target, CurrentTeam, OpponentTeam, _turnManager, _damageCalculator);
                results.Add(result);
            }
            
            return results;
        }
        
        public void NextTurn()
        {
            if (_turnManager.GetRemainingTurns() == 0)
            {
                SwitchTeams();
            }
            else if (ActionOrder.Count > 0)
            {
                var currentUnit = ActionOrder[0];
                ActionOrder.RemoveAt(0);
                ActionOrder.Add(currentUnit);
            }
        }
        
        private void SwitchTeams()
        {
            var temp = CurrentTeam;
            CurrentTeam = OpponentTeam;
            OpponentTeam = temp;
            
            _turnManager.ResetTurns(CurrentTeam.CountAliveUnits());
            CalculateActionOrder();
        }
        
        public bool IsCombatFinished()
        {
            return !CurrentTeam.HasAliveUnits() || !OpponentTeam.HasAliveUnits();
        }
        
        public Team GetWinner()
        {
            if (!OpponentTeam.HasAliveUnits())
                return CurrentTeam;
            if (!CurrentTeam.HasAliveUnits())
                return OpponentTeam;
                
            return null;
        }
        
        public TurnInfo GetCurrentTurns()
        {
            return _turnManager.CurrentTurns;
        }
        
        public void HandleUnitDeath(Unit unit, Team team)
        {
            if (!unit.IsAlive && unit is Monster monster)
            {
                for (int i = 0; i < team.BoardPositions.Length; i++)
                {
                    if (team.BoardPositions[i] == unit)
                    {
                        team.BoardPositions[i] = null;
                        if (!team.Reserve.Contains(monster))
                        {
                            team.Reserve.Add(monster);
                        }
                        break;
                    }
                }
            }
        }
    }