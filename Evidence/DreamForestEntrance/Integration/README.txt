Dream Forest entrance - integration boundary

The entrance is Done, accepted by the user on 2026-11-27 after Art Director review v2.

This commit contains only the entrance module, its imported artwork, tests,
review evidence, and the shared development diary. The Dream Forest stage base
belongs to Antigravity job 229 and was still untracked when this commit was made.
The entrance assemblies require that stage's Core assembly.

The two shared integration files are already patched in the current working tree:
  Assets/Scripts/Runtime/DreamForest/Runtime/DreamForestStageBootstrap.cs
  Assets/Scripts/Runtime/DreamForest/Runtime/YunaLegend.DreamForest.Runtime.asmdef

entrance-integration.patch records only this delivery's additions to those files.
It must not be applied again if the stage base already contains the integration.

Check an already-integrated checkout from the repository root:
  git apply --reverse --check --ignore-space-change Evidence/DreamForestEntrance/Integration/entrance-integration.patch

For a stage base without this integration, check and then apply:
  git apply --check --ignore-space-change Evidence/DreamForestEntrance/Integration/entrance-integration.patch
  git apply --ignore-space-change Evidence/DreamForestEntrance/Integration/entrance-integration.patch

Validation: 26/26 EditMode tests; 40 native Unity renders; 449 PlayMode samples
through the seven authored beats and return to gameplay; native UI event tests
for the local phase selector, pause/resume, and one-gesture narration advance.
Art Director approval: Evidence/DreamForestEntranceArtReview/review-v2.txt.
