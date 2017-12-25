using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionPreferred : SkillSelectionDesire
{
    public SkillSelectionPreferred(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        var monster = performer.Character as Monster;
        if(monster.Data.PreferableSkill >= 0)
        {
            if(monster.Data.CombatSkills.Count > monster.Data.PreferableSkill)
            {
                var skill = monster.Data.CombatSkills[monster.Data.PreferableSkill];
                if (performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) != null)
                    return false;
                if(BattleSolver.IsSkillUsable(performer, skill))
                {
                    decision.Decision = BrainDecisionType.Perform;
                    decision.SelectedSkill = skill;
                    decision.TargetInfo.Targets = BattleSolver.GetSkillAvailableTargets(performer, decision.SelectedSkill);
                    decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.IsSelfTarget ?
                        SkillTargetType.Self : decision.SelectedSkill.TargetRanks.IsSelfFormation ?
                            SkillTargetType.Party : SkillTargetType.Enemy;

                    var availableTargetDesires = new List<TargetSelectionDesire>(monster.Brain.TargetDesireSet);

                    while (availableTargetDesires.Count > 0)
                    {
                        TargetSelectionDesire desire = RandomSolver.ChooseByRandom(availableTargetDesires);
                        if (desire.SelectTarget(performer, decision))
                            return true;
                        else
                            availableTargetDesires.Remove(desire);
                    }
                }
            }
        }
        return false;
    }

    private void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)((double)dataSet["base_chance"] * 100);
                    break;
                default:
                    Debug.LogError("Unknown token in preferred skill desire: " + token.Key);
                    break;
            }
        }
    }
}