package com.example.widget_android.data

enum class ClockingMode {
    WITH_MEAL,
    TWO_BREAKS
}

enum class StepType {
    ENTRY,
    BREAK1_START,
    BREAK1_END,
    MEAL_START,
    MEAL_END,
    BREAK2_START,
    BREAK2_END,
    EXIT
}

data class ClockingStep(
    val type: StepType,
    val label: String
)

data class ClockingState(
    val mode: ClockingMode = ClockingMode.WITH_MEAL,
    val currentStepIndex: Int = 0,
    val isFinished: Boolean = false,
    val lastActionLabel: String = "Sin fichajes todavía",
    val lastActionTime: String = "--:--"
)