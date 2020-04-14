using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public enum AnimationEnum {
    idle,
    move,
    attack,
    die
}

[System.Serializable]
public class AnimationCategory {
    public AnimationEnum category;
    public List<AnimationClip> clips;
}

public class AnimationManager : MonoBehaviour {
    public Animation animator;
    public List<AnimationCategory> animations = new List<AnimationCategory>();
    private List<AnimationClip> move => animations.Where(a => a.category == AnimationEnum.move).Select(a => a.clips).First();
    private List<AnimationClip> die => animations.Where(a => a.category == AnimationEnum.die).Select(a => a.clips).First();
    private List<AnimationClip> attack => animations.Where(a => a.category == AnimationEnum.attack).Select(a => a.clips).First();
    private List<AnimationClip> idle => animations.Where(a => a.category == AnimationEnum.idle).Select(a => a.clips).First();

    public bool isReady { get; private set; }
    public float attackAnimationLength => attack.First().length;

    public float playDeathAnimation() {
        if (!isReady) {
            return 0;
        }
        return getRandomAnimationAndLength("die", die);
    }

    public float playAttackAnimation() {
        if (!isReady) {
            return 0;
        }
        return getRandomAnimationAndLength("attack", attack);
    }

    public float playIdleAnimation() {
        if (!isReady) {
            return 0;
        }
        return getRandomAnimationAndLength("idle", idle);
    }

    public float playMoveAnimation() {
        if (!isReady) {
            return 0;
        }
        return getRandomAnimationAndLength("move", move);
    }

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
