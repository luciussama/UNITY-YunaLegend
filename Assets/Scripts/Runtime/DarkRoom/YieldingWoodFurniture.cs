using UnityEngine;

namespace YunaLegend.DarkRoom
{
    /// <summary>A short physical traversal beat, never a global pause or a timed jump.</summary>
    [DefaultExecutionOrder(100)]
    public class YieldingWoodFurniture : MonoBehaviour
    {
        public const int PlatformIndex = 2;
        public const float StepDelay = .08f;
        public const float RecoveryDuration = .28f;
        [SerializeField] private AudioClip woodCrack;
        private DarkRoomPlatformMarker marker;
        private Yuna25DPlayerController player;
        private NandaFairyGuide fairy;
        private AudioSource audioSource;
        private Transform leftVisual,rightVisual,leftContact,rightContact,splinters;
        private PlatformSupportShape[] restShapes;
        private float sag;
        private void Awake() { Configure(woodCrack);Bind();ResetForRetry(); }
        public bool HasCracked { get; private set; }
        public bool IsTraversing { get; private set; }
        public int CrackCount { get; private set; }
        public float CurrentSag => sag;
        public float RestTopY => transform.position.y+marker.spec.height*.5f;

        public void Configure(AudioClip crack)
        {
            woodCrack=crack;
            audioSource=GetComponent<AudioSource>();
            if(audioSource==null)audioSource=gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake=false;audioSource.spatialBlend=.2f;audioSource.volume=.55f;
        }

        void Bind()
        {
            if(marker==null)marker=GetComponent<DarkRoomPlatformMarker>();
            if(player==null)player=Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if(fairy==null)fairy=Object.FindFirstObjectByType<NandaFairyGuide>();
            if(leftVisual==null || leftContact==null)
            {
                leftVisual=transform.Find("Visual_3D/Tampo_Esquerdo");rightVisual=transform.Find("Visual_3D/Tampo_Direito");
                leftContact=transform.Find("Contacto_Fisico/Tampo_Esquerdo");rightContact=transform.Find("Contacto_Fisico/Tampo_Direito");
                splinters=transform.Find("Visual_3D/Farpas");
                restShapes=DarkRoomPlatformArt.GetSupportShapes(marker.spec);
            }
        }

        private void Update() => Tick(Time.deltaTime);
        private void OnDisable()
        {
            IsTraversing=false;
            if(fairy!=null)fairy.SetTraversalSupport(null);
        }

        public void ResetForRetry()
        {
            Bind();HasCracked=false;CrackCount=0;IsTraversing=false;sag=0;
            if(audioSource!=null)audioSource.Stop();
            ApplyPose(0f,0f);
            if(fairy!=null)fairy.SetTraversalSupport(null);
        }

        public void Tick(float dt)
        {
            Bind();if(player==null||leftVisual==null||dt<=0f)return;
            if(player.CurrentSupportIndex<PlatformIndex&&!player.IsDead)
            {
                if(HasCracked||sag>0f)ResetForRetry();return;
            }
            bool onFurniture=player.CurrentSupportIndex==PlatformIndex&&!player.IsDead;
            float progress=Mathf.Clamp01((player.transform.position.x-(transform.position.x-marker.spec.width*.5f))/marker.spec.width);
            IsTraversing=onFurniture&&progress<.90f;
            if(fairy!=null)fairy.SetTraversalSupport(IsTraversing?marker:null);
            if(onFurniture&&player.IsGrounded&&!HasCracked&&progress>=.48f)
            {
                HasCracked=true;CrackCount++;
                player.BeginStumble(StepDelay,RecoveryDuration);
                if(audioSource!=null&&woodCrack!=null)audioSource.PlayOneShot(woodCrack);
            }
            float desired=HasCracked?.075f:onFurniture&&player.IsGrounded?.018f+.025f*Mathf.Sin(progress*Mathf.PI):0f;
            sag=Mathf.MoveTowards(sag,desired,dt*.18f);
            ApplyPose(sag,HasCracked?.025f:0f);
            if((progress>.98f||player.CurrentSupportIndex>PlatformIndex)&&player.IsStumbling)player.EndStumble();
        }

        public void Preview(float amount,bool cracked)
        {
            Bind();HasCracked=cracked;sag=amount;ApplyPose(amount,cracked?.025f:0f);
        }

        void ApplyPose(float amount,float spread)
        {
            if(leftVisual==null||leftContact==null)return;
            var shapes=restShapes;
            for(int i=0;i<2;i++)
            {
                var shape=shapes[i];float side=i==0?-1f:1f;
                var hinge=new Vector3(side*marker.spec.width*.5f,marker.spec.height*.5f,0);
                float angle=Mathf.Atan2(amount,shape.boxSize.x)*Mathf.Rad2Deg*(i==0?-1f:1f);
                var rotation=Quaternion.Euler(0,0,angle);
                var position=hinge+rotation*(shape.position-hinge)+Vector3.right*side*spread;
                var visual=i==0?leftVisual:rightVisual;var contact=i==0?leftContact:rightContact;
                visual.localPosition=position;visual.localRotation=rotation;
                contact.localPosition=position;contact.localRotation=rotation;
            }
            if(splinters!=null)splinters.gameObject.SetActive(HasCracked);
            Physics.SyncTransforms();
        }
    }
}
