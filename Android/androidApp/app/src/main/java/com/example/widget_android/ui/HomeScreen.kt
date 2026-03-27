package com.example.widget_android.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import android.content.Context
import com.example.widget_android.data.ClockingMode
import com.example.widget_android.data.ClockingRepository

@Composable
fun HomeScreen(context: Context) {
    var state by remember { mutableStateOf(ClockingRepository.getState(context)) }
    val currentStep = ClockingRepository.getCurrentStep(context)

    fun reload() {
        state = ClockingRepository.getState(context)
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(20.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        Text(
            text = "Widget de fichajes",
            style = MaterialTheme.typography.headlineSmall
        )

        Card(
            shape = RoundedCornerShape(16.dp),
            elevation = CardDefaults.cardElevation(defaultElevation = 4.dp),
            modifier = Modifier.fillMaxWidth()
        ) {
            Column(modifier = Modifier.padding(16.dp)) {
                Text("Modo actual:")
                Text(
                    text = when (state.mode) {
                        ClockingMode.WITH_MEAL -> "Comida + 1 descanso"
                        ClockingMode.TWO_BREAKS -> "2 descansos"
                    },
                    style = MaterialTheme.typography.titleMedium
                )

                Spacer(modifier = Modifier.height(12.dp))

                Text("Último fichaje:")
                Text("${state.lastActionLabel} - ${state.lastActionTime}")

                Spacer(modifier = Modifier.height(12.dp))

                Text("Siguiente fichaje:")
                Text(
                    text = if (state.isFinished) "Jornada terminada"
                    else currentStep?.label ?: "No disponible",
                    style = MaterialTheme.typography.titleMedium
                )
            }
        }

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            OutlinedButton(
                onClick = {
                    ClockingRepository.setMode(context, ClockingMode.WITH_MEAL)
                    reload()
                },
                modifier = Modifier.weight(1f)
            ) {
                Text("Con comida")
            }

            OutlinedButton(
                onClick = {
                    ClockingRepository.setMode(context, ClockingMode.TWO_BREAKS)
                    reload()
                },
                modifier = Modifier.weight(1f)
            ) {
                Text("2 descansos")
            }
        }

        Button(
            onClick = {
                ClockingRepository.performNextClocking(context)
                reload()
            },
            modifier = Modifier.fillMaxWidth(),
            enabled = !state.isFinished
        ) {
            Text(
                if (state.isFinished) "Jornada finalizada"
                else "Fichar siguiente"
            )
        }

        OutlinedButton(
            onClick = {
                ClockingRepository.reset(context)
                reload()
            },
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Reiniciar jornada")
        }
    }
}