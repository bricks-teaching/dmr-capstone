using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

/// <summary>
/// Valid animation types. Based on the bots, just these four classes.
/// </summary>
public enum AnimationEnum {
    idle,
    move,
    attack,
    die
}

/// <summary>
/// Data class to reveal fields for the inspector.
/// </summary>
[System.Serializable]
public class AnimationCategory {
    public AnimationEnum category;
    public List<AnimationClip> clips;
}

/// <summary>
/// Manages animations for a bot.
/// </summary>
public class AnimationManager : MonoBehaviour {
    [Tooltip("Reference to this bot's own animation clip. Needed to play animations.")]
    public Animation animator;

    [Tooltip("List of animations to play. Multiples of the same category is not fully supported. Multiple clips per category, aside from death animations, are not fully supported.")]
    public List<AnimationCategory> animations = new List<AnimationCategory>();

    /// <summary>
    /// Reference to move animations
    /// </summary>
    private List<AnimationClip> move => animations.Where(a => a.category == AnimationEnum.move).Select(a => a.clips).First();

    /// <summary>
    /// Reference to death animations
    /// </summary>
    private List<AnimationClip> die => animations.Where(a => a.category == AnimationEnum.die).Select(a => a.clips).First();

    /// <summary>
    /// Reference to attack animations
    /// </summary>
    private List<AnimationClip> attack => animations.Where(a => a.category == AnimationEnum.attack).Select(a => a.clips).First();

    /// <summary>
    /// Reference to idle animations
    /// </summary>
    /// <param name="a.category"></param>
    /// <returns></returns>
    private List<AnimationClip> idle => animations.Where(a => a.category == AnimationEnum.idle).Select(a => a.clips).First();

    /// <summary>
    /// Whether the animation manager is set up and ready to start
    /// serving animation requests.
    /// </summary>
    /// <value></value>
    public bool isReady { get; private set; }

    /// <summary>
    /// Play a random death animation and return the length of the clip played
    /// </summary>
    /// <returns>length of clip played</returns>
    public float playDeathAnimation() {
        if (!isReady) {
            return 0;
        }
        return getRandomAnimationAndLength("die", die);
    }

    /// <summary>
    /// Play a random attack animation and return the length of the clip played
    /// </summary>
    /// <returns>length of clip played</returns>
    public float playAttackAnimation() {
        if (!isReady) {
            return 0;
        }
        return getRandomAnimationAndLength("attack", attack);
    }

    /// <summary>
    /// Play a random idle animation and return the length of the clip played
    /// </summary>
    /// <returns>length of clip played</returns>
    public float playIdleAnimation() {
        if (!isReady) {
            return 0;
        }
        return getRandomAnimationAndLength("idle", idle);
    }

    /// <summary>
    /// Play a random movement animation and return the length of the clip played
    /// </summary>
    /// <returns>length of clip played</returns>
    public float playMoveAnimation() {
        if (!isReady) {
            return 0;
        }
        return getRandomAnimationAndLength("move", move);
    }

    /// <summary>
    /// Set up clips based on what was set in the inspector. Generic
    /// names are needed to do things in other scripts.
    /// </summary>
    void Start() {
        // Add clips to the animator
        for (int i = 0; i < move.Count; ++i) {
            animator.AddClip(move[i], $"move{i}");
        }
        for (int i = 0; i < die.Count; ++i) {
            animator.AddClip(die[i], $"die{i}");
        }
        for (int i = 0; i < idle.Count; ++i) {
            animator.AddClip(idle[i], $"idle{i}");
        }
        for (int i = 0; i < attack.Count; ++i) {
            animator.AddClip(attack[i], $"attack{i}");
        }
        isReady = true;
    }

    /// <summary>
    /// Play a clip and return the length of the selected clip
    /// </summary>
    /// <param name="type">string version of enum, used to select the clip by name (string)</param>
    /// <param name="clips">list of clips to pick from</param>
    /// <returns>length of clip played</returns>
    private float getRandomAnimationAndLength(string type, List<AnimationClip> clips) {
        int idx;
        if (clips.Count == 0) {
            return 0;
        } else if (clips.Count == 1) {
            idx = 0;
        } else {
            idx = UnityEngine.Random.Range(0, clips.Count-1);
        }
        string clipToPlay = $"{type}{idx}";
        if (!animator.IsPlaying(clipToPlay)) {
            animator.Play(clipToPlay);
            return animator.GetClip(clipToPlay).length;
        }
        return 0;
    }
}
