Shader "Custom/2D/OutlineSpriteSoft"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // アウトライン色 (オレンジ寄りに設定してみる例)
        _OutlineColor ("Outline Color", Color) = (1,0.5,0,1)

        // アウトラインの太さ
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.05)) = 0.01

        // アウトラインのソフト度合い
        // 値を大きくすると、隣接ピクセルが1つでもあれば弱く色が乗る
        // 小さくすると、全周がしっかり塗られたときだけ濃くなる
        _OutlineSoftness ("Outline Softness", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "UniversalMaterialType"="2D"
        }
        LOD 100

        Pass
        {
            Name "OutlineSprite"
            Tags
            {
                "LightMode"="Universal2D"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float4 _OutlineColor;
            float  _OutlineThickness;
            float  _OutlineSoftness;

            struct VertexInput
            {
                float4 position : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct FragmentInput
            {
                float4 position : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            FragmentInput vert(VertexInput v)
            {
                FragmentInput o;
                o.position = mul(UNITY_MATRIX_MVP, v.position);
                o.texcoord = v.texcoord;
                return o;
            }

            float4 frag(FragmentInput i) : SV_Target
            {
                // 元のスプライト色
                float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord) * _Color;

                // 現在ピクセルのアルファ
                float alpha = c.a;

                // アウトラインの太さに基づいてUVオフセット
                float2 offset = float2(_OutlineThickness, _OutlineThickness);

                // 8方向サンプル
                float4 up       = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(0, offset.y));
                float4 down     = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - float2(0, offset.y));
                float4 left     = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - float2(offset.x, 0));
                float4 right    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(offset.x, 0));
                float4 upLeft   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(-offset.x, offset.y));
                float4 upRight  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(offset.x, offset.y));
                float4 downLeft = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(-offset.x, -offset.y));
                float4 downRight= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(offset.x, -offset.y));

                // 各サンプルのアルファが 0.5 を超えているかどうか
                float sumAlpha =
                    step(0.5, up.a) +
                    step(0.5, down.a) +
                    step(0.5, left.a) +
                    step(0.5, right.a) +
                    step(0.5, upLeft.a) +
                    step(0.5, upRight.a) +
                    step(0.5, downLeft.a) +
                    step(0.5, downRight.a);

                // 現在のピクセルが (alpha < 0.5) で、近くにアルファ>0.5 ピクセルがあるときがアウトライン判定
                // ただし、"sumAlpha" が大きいほど「輪郭の中でも内部寄り」なので、濃い色をつける
                // "sumAlpha" が小さいほど縁の外側になるので、薄くする
                if (alpha < 0.5 && sumAlpha > 0.0)
                {
                    // 最大8ピクセルが α>0.5 なので、 sumAlpha は 0~8 の範囲
                    // これを 0~1 の範囲に正規化 (sumAlpha / 8) し、さらにソフトネスで調整
                    float outlineFactor = (sumAlpha / 8.0);

                    // _OutlineSoftness で「輪郭のにじみ具合」を調整
                    // 値が小さいほど「sumAlphaが小さいとほぼ見えない」= シャープ
                    // 値が大きいほど「sumAlphaが小さくても輪郭が見える」= ソフト
                    outlineFactor = pow(outlineFactor, 1.0 - _OutlineSoftness);

                    // アウトライン色(オレンジ)のアルファを outlineFactor に応じて変化
                    float4 outlineCol = float4(_OutlineColor.rgb, _OutlineColor.a * outlineFactor);

                    return outlineCol;
                }

                // それ以外は元の色
                return c;
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/Fallback"
}
