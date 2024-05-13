using Godot;
using System;

public partial class ScoreDisplay : Label {
	[Export]
	public int ScoreWidth = 7;

    public override void _Ready () {
        STGController.Instance.ScoreUpdate += UpdateScore;
    }

	public void UpdateScore () {
		string scoreText = STGController.Score.ToString ($"D{ScoreWidth}");
		Text = $"SCORE: {scoreText}";
	}
}
