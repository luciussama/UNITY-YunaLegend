"""Audit real T03.4 PlayMode samples and assemble unretouched render replays."""
import hashlib,json,sys,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(ROOT/'Build/25d/PythonDeps'))
from PIL import Image
E=ROOT/'Evidence/25d/T03.4'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def audit(hz):
 p=E/f'final-v2-{hz}/stumble-proof.json';r=json.loads(p.read_text()); t=r['ticks'];dt=1/hz
 assert r['success'] and r['events']==3 and r['frames']==90
 assert all(r[x] for x in ['resetClean','visualResetClean','timeScaleUnchanged','colliderUnchanged','rootScaleUnchanged'])
 assert r['resetStepCount']==0 and r['resetStepTimer']==0
 assert r['gameplayBefore']==r['gameplayAfter']==sha(ROOT/'Assets/Scenes/DarkRoom_Gameplay.unity')
 assert r['controllerBefore']==r['controllerAfter']==sha(ROOT/'Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs')
 assert r['atlasHash']==sha(ROOT/'Assets/Art/Illustrated25D/Yuna/Stumble/Yuna_Stumble_Atlas_v1.png')
 assert {s['pose'] for s in t if s['pose']>=0}==set(range(8))
 assert all(s['timeScale']==1 for s in t)
 # Controller countdown deltas, not rounded rendered frames. Interrupted second recovery is intentional.
 starts=[i for i,s in enumerate(t) if s['delay']>.07999]
 assert len(starts)==3
 delays=[];recoveries=[];trajectory=[]
 for n,i in enumerate(starts):
  end=next(j for j in range(i+1,len(t)) if t[j]['delay']==0)
  consumed=sum(max(0,t[j-1]['delay']-t[j]['delay']) for j in range(i+1,end+1))
  delays.append(consumed);assert abs(consumed-.08)<1e-6
  for j in range(i+1,end):
   assert abs(t[j]['x']-t[j-1]['x'])<1e-5
   assert abs(t[j]['gait']-t[j-1]['gait'])<1e-5
   assert t[j]['clock']>t[j-1]['clock']
  if n==1:continue
  stop=next(j for j in range(end,len(t)) if not t[j]['stumbling'])
  consumed=sum(max(0,t[j-1]['recoveryRemaining']-t[j]['recoveryRemaining']) for j in range(i+1,stop+1))
  recoveries.append(consumed);assert abs(consumed-.28)<1e-6
  error=max(abs((t[j]['x']-t[i]['x'])-r['runSpeed']*max(0,(j-i)*dt-.08)) for j in range(i,stop+1))
  trajectory.append(error);assert error<1e-4
 drift=0
 for a,b in zip(t,t[1:]):
  if a['contact'] and b['contact'] and a['pose']==b['pose']:
   drift=max(drift,abs(a['footX']-b['footX']))
 vertical=max(abs(s['footY']) for s in t if s['contact'])
 assert drift<1e-5 and vertical<1e-5
 samples=r['samples'];steps=[b['visualX']-a['visualX'] for a,b in zip(samples,samples[1:]) if a['contact'] or b['contact']]
 result={'passed':True,'events':r['events'],'delay_consumed_seconds':delays,'complete_recovery_consumed_seconds':recoveries,
 'trajectory_error_m':max(trajectory),'same_pose_contact_drift_m':drift,'contact_vertical_error_m':vertical,
 'visual_step_min_m':min(steps),'visual_step_max_m':max(steps),'atlas_editor_MiB':r['atlasMiB'],
 'reset_step_count':r['resetStepCount'],'reset_step_timer':r['resetStepTimer'],'poses':8,'capture_frames_per_camera':90}
 (E/f'final-v2-{hz}/verification.json').write_text(json.dumps(result,indent=2)+'\n')
 return result
if __name__=='__main__':
 metrics={str(hz):audit(hz) for hz in [30,60,120]}
 out=E/'review-v2';out.mkdir(exist_ok=True)
 shutil.copy2(E/'final-v2-120/stumble-proof.json',out/'stumble-proof.json')
 raw=ROOT/'Build/25d/StumbleFinalV2-120'; hashes={}
 for group in ['game','detail']:
  files=sorted((raw/group).glob('*.png'));assert len(files)==90
  for f in files:hashes[f'{group}/{f.name}']=sha(f)
  ims=[Image.open(f).convert('RGB') for f in files]
  palette=ims[0].quantize(colors=256,method=Image.Quantize.MEDIANCUT)
  frames=[im.quantize(palette=palette,dither=Image.Dither.NONE) for im in ims]
  durations=[(round((i+1)*100/30)-round(i*100/30))*10 for i in range(90)]
  frames[0].save(out/f'stumble-{group}.gif',save_all=True,append_images=frames[1:],duration=durations,loop=0,disposal=2,optimize=False)
  if group=='detail':frames[0].save(out/'stumble-detail-slow.gif',save_all=True,append_images=frames[1:],duration=[d*2 for d in durations],loop=0,disposal=2,optimize=False)
 for f in (E/'final-v2-120').glob('pose-*.png'):shutil.copy2(f,out/f.name)
 (out/'verification.json').write_text(json.dumps({'metrics':metrics,'source_frame_sha256':hashes,'presentation':'Raw Unity renders, fixed GIF palette only; 90 frames/3 seconds normal, 6 seconds slow. Replay includes full event, interrupted reset, repeated event. Restart is a replay, not continuous loop.'},indent=2)+'\n')
 print(json.dumps(metrics,indent=2))
