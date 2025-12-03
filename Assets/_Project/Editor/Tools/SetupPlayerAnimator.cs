using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace ProjectRoguelike.Editor.Tools
{
    public static class SetupPlayerAnimator
    {
        [MenuItem("Tools/Setup Player Animator (Add Run & Jump)")]
        public static void SetupAnimator()
        {
            string animatorPath = "Assets/_Project/Animations/PlayerAnimator.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorPath);
            
            if (controller == null)
            {
                Debug.LogError($"[SetupPlayerAnimator] Could not find Animator Controller at {animatorPath}");
                return;
            }

            // Ajouter le paramètre IsGrounded si il n'existe pas
            bool hasIsGrounded = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == "IsGrounded")
                {
                    hasIsGrounded = true;
                    break;
                }
            }

            if (!hasIsGrounded)
            {
                controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
                Debug.Log("[SetupPlayerAnimator] Added 'IsGrounded' parameter");
            }

            // Obtenir la state machine de base
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            // Trouver les états existants
            AnimatorState idleState = null;
            AnimatorState walkState = null;
            AnimatorState runState = null;
            AnimatorState jumpState = null;

            foreach (var state in stateMachine.states)
            {
                if (state.state.name == "idle")
                    idleState = state.state;
                else if (state.state.name == "walk")
                    walkState = state.state;
                else if (state.state.name == "run")
                    runState = state.state;
                else if (state.state.name == "jump")
                    jumpState = state.state;
            }

            // Charger les animations
            string runAnimPath = "Assets/_Project/Animations/biped/Animation_Running_frame_rate_60.fbx";
            string jumpAnimPath = "Assets/_Project/Animations/biped/Animation_Regular_Jump_frame_rate_60.fbx";
            
            AnimationClip runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(runAnimPath);
            AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(jumpAnimPath);

            if (runClip == null)
            {
                Debug.LogWarning($"[SetupPlayerAnimator] Could not find run animation at {runAnimPath}. Make sure the animation is configured (Rig: Generic, Loop Time: checked).");
            }

            if (jumpClip == null)
            {
                Debug.LogWarning($"[SetupPlayerAnimator] Could not find jump animation at {jumpAnimPath}. Make sure the animation is configured (Rig: Generic).");
            }

            // Créer l'état run s'il n'existe pas
            if (runState == null && runClip != null)
            {
                runState = stateMachine.AddState("run");
                runState.motion = runClip;
                runState.speed = 1f;
                Debug.Log("[SetupPlayerAnimator] Created 'run' state");
            }

            // Créer l'état jump s'il n'existe pas
            if (jumpState == null && jumpClip != null)
            {
                jumpState = stateMachine.AddState("jump");
                jumpState.motion = jumpClip;
                jumpState.speed = 1f;
                Debug.Log("[SetupPlayerAnimator] Created 'jump' state");
            }

            // Supprimer les anciennes transitions pour les recréer proprement
            if (idleState != null && walkState != null)
            {
                // Supprimer les transitions existantes
                var transitionsToRemove = new System.Collections.Generic.List<AnimatorStateTransition>();
                foreach (var transition in idleState.transitions)
                {
                    transitionsToRemove.Add(transition);
                }
                foreach (var transition in transitionsToRemove)
                {
                    idleState.RemoveTransition(transition);
                }

                transitionsToRemove.Clear();
                foreach (var transition in walkState.transitions)
                {
                    transitionsToRemove.Add(transition);
                }
                foreach (var transition in transitionsToRemove)
                {
                    walkState.RemoveTransition(transition);
                }

                // Nouvelles transitions depuis idle
                // idle -> walk (Speed > 0.1 && Speed < 5.0)
                var idleToWalk = idleState.AddTransition(walkState);
                idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
                idleToWalk.AddCondition(AnimatorConditionMode.Less, 5.0f, "Speed");
                idleToWalk.hasExitTime = false;
                idleToWalk.duration = 0.25f;

                // idle -> run (Speed >= 5.0, using Greater with 4.99f)
                if (runState != null)
                {
                    var idleToRun = idleState.AddTransition(runState);
                    idleToRun.AddCondition(AnimatorConditionMode.Greater, 4.99f, "Speed");
                    idleToRun.hasExitTime = false;
                    idleToRun.duration = 0.25f;
                }

                // idle -> jump (!IsGrounded)
                if (jumpState != null)
                {
                    var idleToJump = idleState.AddTransition(jumpState);
                    idleToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
                    idleToJump.hasExitTime = false;
                    idleToJump.duration = 0.1f;
                }

                // Nouvelles transitions depuis walk
                // walk -> idle (Speed < 0.1)
                var walkToIdle = walkState.AddTransition(idleState);
                walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
                walkToIdle.hasExitTime = false;
                walkToIdle.duration = 0.25f;

                // walk -> run (Speed >= 5.0, using Greater with 4.99f)
                if (runState != null)
                {
                    var walkToRun = walkState.AddTransition(runState);
                    walkToRun.AddCondition(AnimatorConditionMode.Greater, 4.99f, "Speed");
                    walkToRun.hasExitTime = false;
                    walkToRun.duration = 0.25f;
                }

                // walk -> jump (!IsGrounded)
                if (jumpState != null)
                {
                    var walkToJump = walkState.AddTransition(jumpState);
                    walkToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
                    walkToJump.hasExitTime = false;
                    walkToJump.duration = 0.1f;
                }
            }

            // Transitions depuis run
            if (runState != null)
            {
                // run -> idle (Speed < 0.1)
                if (idleState != null)
                {
                    var runToIdle = runState.AddTransition(idleState);
                    runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
                    runToIdle.hasExitTime = false;
                    runToIdle.duration = 0.25f;
                }

                // run -> walk (Speed > 0.1 && Speed < 5.0)
                if (walkState != null)
                {
                    var runToWalk = runState.AddTransition(walkState);
                    runToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
                    runToWalk.AddCondition(AnimatorConditionMode.Less, 5.0f, "Speed");
                    runToWalk.hasExitTime = false;
                    runToWalk.duration = 0.25f;
                }

                // run -> jump (!IsGrounded)
                if (jumpState != null)
                {
                    var runToJump = runState.AddTransition(jumpState);
                    runToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
                    runToJump.hasExitTime = false;
                    runToJump.duration = 0.1f;
                }
            }

            // Transitions depuis jump
            if (jumpState != null)
            {
                // jump -> idle (IsGrounded && Speed < 0.1)
                if (idleState != null)
                {
                    var jumpToIdle = jumpState.AddTransition(idleState);
                    jumpToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
                    jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
                    jumpToIdle.hasExitTime = false;
                    jumpToIdle.duration = 0.1f;
                }

                // jump -> walk (IsGrounded && Speed > 0.1 && Speed < 5.0)
                if (walkState != null)
                {
                    var jumpToWalk = jumpState.AddTransition(walkState);
                    jumpToWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
                    jumpToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
                    jumpToWalk.AddCondition(AnimatorConditionMode.Less, 5.0f, "Speed");
                    jumpToWalk.hasExitTime = false;
                    jumpToWalk.duration = 0.1f;
                }

                // jump -> run (IsGrounded && Speed >= 5.0, using Greater with 4.99f)
                if (runState != null)
                {
                    var jumpToRun = jumpState.AddTransition(runState);
                    jumpToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
                    jumpToRun.AddCondition(AnimatorConditionMode.Greater, 4.99f, "Speed");
                    jumpToRun.hasExitTime = false;
                    jumpToRun.duration = 0.1f;
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[SetupPlayerAnimator] Animator Controller updated successfully!");
        }
    }
}

