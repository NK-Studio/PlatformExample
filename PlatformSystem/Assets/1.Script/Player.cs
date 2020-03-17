using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AnimatorPro;

[RequireComponent(typeof(AnimatorPro))]
public class Player : MonoBehaviour
{
    #region Show Inspector

    [SerializeField, Tooltip("애니메이터 참조 변수")]
    private Animator animator;

    [SerializeField, Tooltip("이동 속도")]
    private float moveSpeed = 1f;

    [SerializeField, Tooltip("점프 속도")]
    private float jumpSpeed = 3f;

    [SerializeField, Tooltip("점프시 중력")]
    private float jumpGravitiy;

    [SerializeField, Tooltip("낙하시 중력")]
    private float fallGravitiy;

    [SerializeField, Tooltip("땅 체크 마스크")]
    private LayerMask GroundMask;
    
    #endregion

    #region Hide Inspector

    //박스콜라이더 참조 변수
    private CapsuleCollider2D bodyCollider;

    //애니메이터 프로 참조 변수
    private AnimatorPro animatorPro;

    //리지드바디2D 참조 변수
    private new Rigidbody2D rigidbody2D;

    //이동 제어를 하는 변수
    private bool canMove = true;

    //좌우 이동에 대한 변수
    private float h;
    
    //땅 체크 변수
    private bool isGround;

    #endregion

    #region Macro

    private static readonly int ID_Move = Animator.StringToHash("Move");
    private static readonly int ID_isJump = Animator.StringToHash("isJump");
    private static readonly int ID_isFall = Animator.StringToHash("isFall");
    private static readonly int ID_isGround = Animator.StringToHash("isGround");
    private static readonly int ID_Velocity_Y = Animator.StringToHash("Velocity_Y");
    private static readonly int ID_isSit = Animator.StringToHash("isSit");

    #endregion

    private void Awake()
    {
        if (animator == null) return;

        animatorPro = GetComponent<AnimatorPro>();
        animatorPro.Init(animator);

        bodyCollider = GetComponent<CapsuleCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //방향키 입력
        h = Input.GetAxisRaw("Horizontal");
        
        //서있기, 달리기 애니메이션 전환
        animatorPro.SetParam(ID_Move, h);
    }

    private void FixedUpdate()
    {
        //이동
        updateMove(h);
        
        //좌우 전환
        updateFlip(h);

        //땅 체크
        updateGroundCheck();
        
        //점프, 낙하 중력 변환
        updateGravitiyScale();
        
        //점프
        updateJump();
        
        //낙하
        updateFall();
        
        //앉기
        updateSit();
    }

    #region Other

    private void updateGroundCheck()
    {
        isGround = bodyCollider.IsTouchingLayers(GroundMask);

        //땅에 닿으면, 낙하를 false한다.
        if (isGround)
            animatorPro.SetParam(ID_isFall,false);
        
        //땅 충돌을 실시간으로 애니메이터에 넘긴다.
        animatorPro.SetParam(ID_isGround, isGround);
    }
    
    private void updateGravitiyScale()
    {
        //디폴트는 1이다.
        rigidbody2D.gravityScale = 1f;

        //위로 상승하면 jump, 하강이면 fall 중력으로 변경한다.
        if (rigidbody2D.velocity.y > 0f)
            rigidbody2D.gravityScale = jumpGravitiy;
        else if (rigidbody2D.velocity.y < 0f)
            rigidbody2D.gravityScale = fallGravitiy;
        
        //실시간으로 애니메이터에 액터의 y의 힘을 넘겨준다.
        animatorPro.SetParam(ID_Velocity_Y, rigidbody2D.velocity.y);
    }

    #endregion

    #region playerUpadate
    
    private void updateFlip(float Axis)
    {
        //h가 0.0f가 아니라면, (h != 0f 이렇게 조건하는 것은 효율이 좋지 않습니다.)
        if (!Mathf.Approximately(Axis, 0f))
            transform.localScale = new Vector2(Axis, 1f);
    }
    
    private void updateMove(float Axis)
    {
        //이동 처리
        var vel = rigidbody2D.velocity;

        //이동 가능시에만 값 변경
        if (canMove)
            vel.x = Axis * (moveSpeed * 100) * Time.deltaTime;
        else
            vel.x = 0f;

        //적용
        rigidbody2D.velocity = vel;
    }

    private void updateJump()
    {
        //점프 상태이거나, 스페이스바를 안눌렀다면, 아래 코드 구문 실행 X
        if (animatorPro.GetParam<bool>(ID_isJump) || !Input.GetKey(KeyCode.W)) return;

        //점프 상태로 변경
        animatorPro.SetParam(ID_isJump, true);
        
        //실제 액터를 위로 점프 시킴
        rigidbody2D.AddForce(Vector2.up * jumpSpeed,ForceMode2D.Impulse);
    }
    
    private void updateFall()
    {
        //땅에 있지 않고, 떨어지는 중이라면,
        if(!isGround && rigidbody2D.velocity.y < 0.0f)
            animatorPro.SetParam(ID_isFall,true);
    }
    
    private void updateSit()
    {
        //키를 누르고 않누르고를 canMove의 움직일 수 없다, 있다로 처리함.

        var isSit = false;

        if (Input.GetKey(KeyCode.S))
        {
            isSit = true;
            canMove = false;
        }
        else
            canMove = true;
        
        animatorPro.SetParam(ID_isSit, isSit);
    }
    
    #endregion

    #region animState

    [AnimatorExit("Base Layer.Jump&Fall.Fall")]
    public void JumpEnd() =>
        animatorPro.SetParam(ID_isJump,false);

    #endregion
}
