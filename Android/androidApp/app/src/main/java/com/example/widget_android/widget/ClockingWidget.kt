package com.example.widget_android.widget

import android.content.Context
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.glance.GlanceId
import androidx.glance.GlanceModifier
import androidx.glance.appwidget.GlanceAppWidget
import androidx.glance.appwidget.SizeMode
import androidx.glance.appwidget.provideContent
import androidx.glance.background
import androidx.glance.layout.Column
import androidx.glance.layout.Spacer
import androidx.glance.layout.fillMaxSize
import androidx.glance.layout.height
import androidx.glance.layout.padding
import androidx.glance.text.FontWeight
import androidx.glance.text.Text
import androidx.glance.text.TextStyle
import androidx.glance.unit.ColorProvider
import com.example.widget_android.data.ClockingMode
import com.example.widget_android.data.ClockingRepository

class ClockingWidget : GlanceAppWidget() {

    override val sizeMode = SizeMode.Single

    override suspend fun provideGlance(context: Context, id: GlanceId) {
        val state = ClockingRepository.getState(context)
        val currentStep = ClockingRepository.getCurrentStep(context)

        provideContent {
            Column(
                modifier = GlanceModifier
                    .fillMaxSize()
                    .background(ColorProvider(Color.White))
                    .padding(16.dp)
            ) {
                Text(
                    text = "Fichajes",
                    style = TextStyle(
                        color = ColorProvider(Color(0xFF5F96F9)),
                        fontWeight = FontWeight.Bold
                    )
                )

                Spacer(modifier = GlanceModifier.height(8.dp))

                Text(
                    text = when (state.mode) {
                        ClockingMode.WITH_MEAL -> "Modo: comida"
                        ClockingMode.TWO_BREAKS -> "Modo: 2 descansos"
                    }
                )

                Spacer(modifier = GlanceModifier.height(8.dp))

                Text(text = "Ultimo:")
                Text(text = "${state.lastActionLabel} - ${state.lastActionTime}")

                Spacer(modifier = GlanceModifier.height(8.dp))

                Text(text = "Siguiente:")
                Text(
                    text = if (state.isFinished) "Jornada terminada"
                    else currentStep?.label ?: "No disponible",
                    style = TextStyle(fontWeight = FontWeight.Bold)
                )
            }
        }
    }
}