using TnT.EduGame.Characters;
using TnT.EduGame.GameState;
using Godot;
using TnT.Extensions;

public partial class CharacterSelector : Node
{
    Character _playerCharacter;
    void Start()
    {
        var player = GetTree().FindAnyObjectByType<Player>();
        _playerCharacter = player.GetTree().FindAnyObjectByType<Character>();
    }
    public void SelectCharacter(CharacterData characterData)
    {
        _playerCharacter.LoadCharacter(characterData.Id);

        // StateManagerGame.Instance.LoadScene("Home", new(9, -1));
    }
}