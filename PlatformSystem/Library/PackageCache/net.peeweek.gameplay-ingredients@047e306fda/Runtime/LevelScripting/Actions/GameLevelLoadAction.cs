using System.Linq;
using UnityEngine;
using NaughtyAttributes;

namespace GameplayIngredients.Actions
{
    public class GameLevelLoadAction : ActionBase
    {
        public enum Target
        {
            MainMenu,
            First,
            Previous,
            Current,
            Next,
            Last,
            SpecifiedLevel,
            FromGameSave,
        }
        public bool ShowUI = true;
        public Target level = Target.First;
        [NonNullCheck, ShowIf("isSpecified"), Tooltip("Which Level to Load/Unload, when selected 'Specified' level")]
        public GameLevel specifiedLevel;

        [ShowIf("isGameSave")]
        public int UserSaveIndex = 0;
        [ShowIf("isGameSave")]
        public string UserSaveName = "Progress";

        public bool SaveProgress = false;

        [ReorderableList]
        public Callable[] OnComplete;

        private bool isSpecified() { return level == Target.SpecifiedLevel; }
        private bool isGameSave() { return level == Target.FromGameSave; }

        public override void Execute(GameObject instigator = null)
        {
            
        }
    }
}
