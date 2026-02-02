using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;

/// <summary>
/// 한글 TMP SDF 폰트 에셋 생성 에디터 메뉴.
/// </summary>
public static class SetupKoreanFont
{
    private const string SavePath = "Assets/Fonts/MalgunGothic SDF.asset";
    private const string FontPath = "Assets/Fonts/MalgunGothic.ttf";

    [MenuItem("LoveSimulation/Generate Korean TMP Font")]
    public static void GenerateKoreanFont()
    {
        // 기존 에셋 삭제
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SavePath) != null)
        {
            AssetDatabase.DeleteAsset(SavePath);
        }

        // 폰트 파일 로드
        var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null)
        {
            Debug.LogError($"[SetupKoreanFont] {FontPath}를 찾을 수 없습니다.");
            return;
        }

        // TMP 폰트 에셋 생성 (아틀라스 크기 4096)
        var fontAsset = TMP_FontAsset.CreateFontAsset(
            font, 36, 5, GlyphRenderMode.SDFAA, 4096, 4096);

        if (fontAsset == null)
        {
            Debug.LogError("[SetupKoreanFont] TMP_FontAsset 생성 실패.");
            return;
        }

        fontAsset.name = "MalgunGothic SDF";

        // 메인 에셋 저장
        AssetDatabase.CreateAsset(fontAsset, SavePath);

        // 아틀라스 텍스처를 서브에셋으로 저장
        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = "MalgunGothic SDF Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        // 머티리얼을 서브에셋으로 저장
        if (fontAsset.material != null)
        {
            fontAsset.material.name = "MalgunGothic SDF Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        AssetDatabase.SaveAssets();

        // 한글 문자 추가
        string allChars = GetKoreanAndBasicCharacters();
        bool success = fontAsset.TryAddCharacters(allChars, out string missingChars);

        // 추가된 아틀라스 텍스처 페이지도 서브에셋으로 저장
        if (fontAsset.atlasTextures != null)
        {
            for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
            {
                var tex = fontAsset.atlasTextures[i];
                if (tex != null && !AssetDatabase.Contains(tex))
                {
                    tex.name = $"MalgunGothic SDF Atlas {i}";
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                }
            }
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();

        int missingCount = string.IsNullOrEmpty(missingChars) ? 0 : missingChars.Length;
        Debug.Log($"[SetupKoreanFont] 폰트 생성 완료: {SavePath} (성공: {success}, 누락: {missingCount})");

        // TMP 기본 폰트로 설정
        SetAsDefaultFont();

        AssetDatabase.Refresh();
    }

    private static string GetKoreanAndBasicCharacters()
    {
        return
            "가각간갈감갑값강개객거건걸검겁것게겨격견결겸경곁계고곡곤골곧공과관광괜괴교구국군굳굴궁권귀규균귤그극근글금급긍기긴길김깊까깎깐깔깜깝깨꺼꺾껍껏껑께껴꼬꼭꼴꼼꼽꽂꽃꽉꽤꾸꾼꿀꿈꿩뀌끄끈끌끓끔끗끝끼낌나낙낚난날낡남납낫낭낮낯낱낳내냄냇냉너넉넌널넓넘넣네넥넷녀녁년념녕노녹논놀놈농높놓놔뇌뇨누눈눌눔눕눠뉘뉴늄느늑는늘늙능늦늬니닉닌닐님닢다닥단닫달닭닮담답닷당닿대댁댐댓더덕던덜덟덤덥덧덩덮데델도독돈돌돕돗동돼되된될됨두둑둘둠둡둥뒤뒷듀드득든들듦듬듭듯등디딕딘딛딜딤딥딩딪따딱딴딸땀땅때땜떠떡떤떨떻떼또똑똘똥뚜뚝뚫뚱뛰뜀뜨뜩뜬뜯뜰뜻띄띠라락란랄람랍랏랑래랙랜램랩랫략량러럭런럴럼럽럿렁레렉렌렘렛려력련렬렴렵령례로록론롤롬롭롯롱뢰료룡루룩룬룰룸룹룻룽뤄류륙률륨륭르른를름릇릉릎리릭린릴림립릿링마막만많말맑맘맙맛망맞맡맣매맥맨맵맺머먹먼멀멈멋멍멎메멘멜멤멧며멱면멸명몇모목몫몬몰몸몹못몽묘무묵묶문묻물묽묾뭄뭇뭐뭘뭣뮈뮤므믈미믹민믿밀밉밋밌밍및밑바박밖반받발밝밟밤밥방밭배백밴밸뱀뱃뱉버번벌범법벗벙벚베벤벨벼벽변별볍병볕보복볶본볼봄봇봉봐봤부북분불붉붐붓붕붙뷔브블비빈빌빔빗빚빛빠빡빤빨빵빼뺏뺨뻐뻔뻗뻣뻤뼈뼘뽀뽑뽕뿌뿐뿔뿜쁘쁨사삭산살삶삼삿상새색샌생샤서석섞선설섬섭섯성세셈셋셔션소속손솔솜솟송솥쇄쇠쇼수숙순술숨숫숭숲쉬쉰쉽슈스슨슬슴습슷승시식신싣실싫심십싯싱싶싸싹싼쌀쌍쌓써썩썰썹쎄쏘쏟쏠쏴쐐쑤쑥쓰쓴쓸씀씌씨씩씬씹씻아악안앉않알앓암압앗앙앞애액앤앨야약얀얄얇양어억언얹얻얼엄업없엇엉엊엌엎에엔엘여역연열염엽엿영옆예옛오옥온올옮옳옷옹와완왈왕왜왠외왼요욕용우욱운울움웃웅워원월웨웬위윗유육윤율으윽은을읊음읍응의이익인일읽잃임입잇있잊잎자작잔잘잠잡잣장잦재쟁저적전절젊점접젓정젖제젠젯져조족존졸좀좁종좋좌죄주죽준줄줌줍중쥐즈즉즌즐즘증지직진짇질짐집짓징짙짚짜짝짠짧짬짱째쨌쩌쩍쩐쩔쩜쪘쪽쫓쭈쭉찌찍찐찔찜찝차착찬찮찰참찻창찾채책챔챕챙처척천철첨첩첫청체쳐초촉촌촘촛총촬최추축춘출춤춥춧충취츠측츨층치칙친칠침칩칭카칸칼캄캐캔캠커컨컬컴컵컷케켓켜코콘콜콤콩쾌쿄쿠쿤퀴크큰클큼키킨킬킴킵킷킹타탁탄탈탐탑탓탕태택탤터턱턴털텀텃텅테텐텔템토톤톨톱통퇴투툴툼퉁튀튜트특튼틀틈틔티틱팀팅파팎판팔팜팝팥패팬퍼퍽페펜펴편펼평폐포폭폰표푸푹풀풍프플피픽필핏핑하학한할함합항해핵핸햄햇행향허헌헐험헛헝헤헬혀현혈협형혜호혹혼홀홈홉홍화확환활황회획횟횡효후훈훌훔훗훨휘휴흉흐흑흔흘흙흠흡흥흩히힌힐힘힙" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
            "0123456789 !#$%&()*+,-./:;<=>?@[]^_{|}~" +
            "\u2026\u00B7\u300C\u300D\u300E\u300F\u3010\u3011\u201C\u201D\u2018\u2019";
    }

    private static void SetAsDefaultFont()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SavePath);
        if (fontAsset == null)
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("TMP Settings");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(path);
            if (settings == null)
            {
                continue;
            }

            var so = new SerializedObject(settings);
            var defaultFontProp = so.FindProperty("m_defaultFontAsset");
            if (defaultFontProp != null)
            {
                defaultFontProp.objectReferenceValue = fontAsset;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
                Debug.Log("[SetupKoreanFont] TMP 기본 폰트를 MalgunGothic SDF로 설정 완료.");
            }
            break;
        }
    }
}
