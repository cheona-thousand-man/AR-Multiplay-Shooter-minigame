using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientAuthNetworkTransform : NetworkTransform
{
    // transform 컴포넌트에 대한 접근 권한 설정. 여기서는 소유한 클라이언트만 가능
    // 서버에서 실행되는 문구인데, 클라이언트에게 권한 부여. 보안에 취약하지 않은 기능만 사용할 것
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
