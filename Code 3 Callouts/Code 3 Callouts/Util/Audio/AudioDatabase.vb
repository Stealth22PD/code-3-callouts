Namespace Util.Audio

    Public Module AudioDatabase

        Public Class DISPATCH

            Enum ATTENTION
                ATTENTION_ALL_UNITS
                SUSPECT_IN_CUSTODY
                DISPATCH_CALLING_UNIT
            End Enum

            Enum DIVISION
                DIV_01
                DIV_02
                DIV_03
                DIV_04
                DIV_05
                DIV_06
                DIV_07
                DIV_08
                DIV_09
                DIV_10
            End Enum

            Enum UNIT_TYPE
                ADAM
                BOY
                CHARLES
                DAVID
                EDWARD
                FRANK
                GEORGE
                HENRY
                IDA
                JOHN
                KING
                LINCOLN
                MARY
                NORA
                OCEAN
                PAUL
                QUEEN
                ROBERT
                SAM
                TOM
                UNION
                VICTOR
                WILLIAM
                XRAY
                YOUNG
                ZEBRA
            End Enum

            Enum BEAT
                BEAT_01
                BEAT_02
                BEAT_03
                BEAT_04
                BEAT_05
                BEAT_06
                BEAT_07
                BEAT_08
                BEAT_09
                BEAT_10
                BEAT_11
                BEAT_12
                BEAT_13
                BEAT_14
                BEAT_15
                BEAT_16
                BEAT_17
                BEAT_18
                BEAT_19
                BEAT_20
                BEAT_21
                BEAT_22
                BEAT_23
                BEAT_24
            End Enum

            Enum REPORTING
                CITIZENS_REPORT
                OFFICERS_REPORT
                UNITS_REPORTING
                WE_HAVE
                WEVE_GOT
            End Enum

            Public Enum CRIMES
                ASSAULT
                CIV_ASSISTANCE
                CODE_211
                CODE_240
                CODE_417
                CODE_459
                CODE_480
                CODE_484
                HIT_AND_RUN
                POSSIBLE_211
                POSSIBLE_459
                POSSIBLE_480
                SUSPICIOUS_VEHICLE
                CODE_390
                POSSIBLE_390
                PUBLIC_INTOX
                PERSON_WITH_FIREARM
                OFFICER_NOT_RESPONDING
                MOTOR_VEHICLE_ACCIDENT
                A_DUI
                DRIVER_UNDER_INFLUENCE
                POSSIBLE_502
                CODE_502
                CODE_187
                HOMICIDE
                OFFICER_IN_NEED_OF_ASSISTANCE
                ASSAULT_ON_A_CIVILIAN
                MUGGING
                CARJACKING
                PERSON_STEALING_A_CAR
            End Enum

            Enum CONJUNCTIVES
                IN_OR_ON_POSITION
                POSITION
                AREA
            End Enum

            Enum REPORT_RESPONSE
                ROGER
                ROGER_THAT
                TEN_FOUR
                TEN_FOUR_COPY_THAT
            End Enum

            Enum RESPONSE_TYPES
                ALL_UNITS_RESPOND_CODE_99_EMERGENCY
                CODE_99_ALL_UNITS_RESPOND
                RESPOND_CODE_2
                RESPOND_CODE_3
                UNITS_RESPOND_CODE_2
                UNITS_RESPOND_CODE_3
                WE_ARE_CODE_4
                WE_ARE_CODE_4A
                NO_FURTHER_UNITS_REQUIRED
            End Enum
            
        End Class

        Class OFFICER
            Enum RESPONDING
                COPY_IN_VICINITY
                ROGER_EN_ROUTE
                ROGER_ON_OUR_WAY
            End Enum

            Enum SUSPECT_SPOTTED
                GOT_EYES_IN_PURSUIT
                HAVE_A_VISUAL
                SUSPECT_IN_SIGHT
                SUSPECT_LOCATED_ENGAGING
            End Enum

            Enum AI_UNIT_RESPONDING
                AI_UNIT_RESPONDING_01
                AI_UNIT_RESPONDING_02
                AI_UNIT_RESPONDING_03
                AI_UNIT_RESPONDING_04
                AI_UNIT_RESPONDING_05
                AI_UNIT_RESPONDING_06
                AI_UNIT_RESPONDING_07
                AI_UNIT_RESPONDING_08
                AI_UNIT_RESPONDING_09
                AI_UNIT_RESPONDING_10
                AI_UNIT_RESPONDING_11
                AI_UNIT_RESPONDING_12
                AI_UNIT_RESPONDING_13
                AI_UNIT_RESPONDING_14
                AI_UNIT_RESPONDING_15
                AI_UNIT_RESPONDING_16
                AI_UNIT_RESPONDING_17
                AI_UNIT_RESPONDING_18
                AI_UNIT_RESPONDING_19
                AI_UNIT_RESPONDING_20
                AI_UNIT_RESPONDING_21
                AI_UNIT_RESPONDING_22
                AI_UNIT_RESPONDING_23
                AI_UNIT_RESPONDING_24
                AI_UNIT_RESPONDING_25
                AI_UNIT_RESPONDING_26
                AI_UNIT_RESPONDING_27
                AI_UNIT_RESPONDING_28
                AI_UNIT_RESPONDING_29
                AI_UNIT_RESPONDING_30
                AI_UNIT_RESPONDING_31
                AI_UNIT_RESPONDING_32
                AI_UNIT_RESPONDING_33
                AI_UNIT_RESPONDING_34
                AI_UNIT_RESPONDING_35
                AI_UNIT_RESPONDING_36
                AI_UNIT_RESPONDING_37
                AI_UNIT_RESPONDING_38
                AI_UNIT_RESPONDING_39
                AI_UNIT_RESPONDING_40
                AI_UNIT_RESPONDING_41
            End Enum
        End Class

    End Module

End Namespace