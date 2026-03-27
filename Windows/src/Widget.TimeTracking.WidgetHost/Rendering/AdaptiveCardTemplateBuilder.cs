using System.Text.Json;
using Widget.TimeTracking.Core.Enums;

namespace Widget.TimeTracking.WidgetHost.Rendering;

internal static class AdaptiveCardTemplateBuilder
{
    /// <summary>Fondo de la tarjeta completa: rectángulo blanco sin redondeo (sin transparencia en esquinas; evita el marco gris del host).</summary>
    private const string WhiteFillBg =
        "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='4' height='4'%3E%3Crect width='4' height='4' fill='%23FFFFFF'/%3E%3C/svg%3E";

    /// <summary>Píldora del contador: blanco con esquinas redondeadas (solo en el bloque del timer).</summary>
    private const string WhiteCardBg =
        "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='160' height='160'%3E%3Crect width='160' height='160' rx='24' fill='%23FFFFFF'/%3E%3C/svg%3E";

    #region Composite button SVGs (background rect + white icon baked into one SVG)

    // Each button is a 44x44 SVG with a rounded-rect fill and the icon paths centered inside.
    // This avoids backgroundImage issues in the widget host and guarantees no deformation.

    private static readonly string EntryBlue = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#5F96F9'/>"
        + "<g transform='translate(10,10) scale(1.6)'>"
        + "<path d='M11.0255 4.9058C13.6582 6.33196 13.6582 8.66804 11.0255 10.0942L8.90142 11.2429L6.77738 12.3916C4.15154 13.8177 2 12.6497 2 9.79736V7.5V5.20264C2 2.35031 4.15154 1.18227 6.78425 2.60844L8.33089 3.44736' stroke='white' stroke-width='1.5' stroke-miterlimit='10' stroke-linecap='round' stroke-linejoin='round' fill='none'/>"
        + "</g></svg>");

    private static readonly string EntryDisabled = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#DDE3EC'/>"
        + "<g transform='translate(10,10) scale(1.6)'>"
        + "<path d='M11.0255 4.9058C13.6582 6.33196 13.6582 8.66804 11.0255 10.0942L8.90142 11.2429L6.77738 12.3916C4.15154 13.8177 2 12.6497 2 9.79736V7.5V5.20264C2 2.35031 4.15154 1.18227 6.78425 2.60844L8.33089 3.44736' stroke='white' stroke-width='1.5' stroke-miterlimit='10' stroke-linecap='round' stroke-linejoin='round' fill='none'/>"
        + "</g></svg>");

    private static readonly string StopBlue = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#5F96F9'/>"
        + "<g transform='translate(8,8)'>"
        + "<rect x='4' y='4' width='20' height='20' rx='5' stroke='white' stroke-width='2.5' fill='none'/>"
        + "</g></svg>");

    private static readonly string StopDisabled = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#DDE3EC'/>"
        + "<g transform='translate(8,8)'>"
        + "<rect x='4' y='4' width='20' height='20' rx='5' stroke='white' stroke-width='2.5' fill='none'/>"
        + "</g></svg>");

    private static readonly string CoffeeBlue = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#5F96F9'/>"
        + "<g transform='translate(8,7) scale(1.85)' fill='none' stroke='white' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'>"
        + "<path d='M1.25 8.468V6.548C1.25 5.088 2.43 3.918 3.88 3.918H8.49C9.95 3.918 11.12 5.098 11.12 6.548V11.128C11.12 12.588 9.94 13.758 8.49 13.758H3.88C2.43 13.758 1.25 12.578 1.25 11.128'/>"
        + "<path d='M3.44 1.968V1.408'/>"
        + "<path d='M5.94 1.968V1.408'/>"
        + "<path d='M8.44 1.968V1.408'/>"
        + "<path d='M13.75 8.228C13.75 9.678 12.57 10.858 11.12 10.858V5.598C12.57 5.598 13.75 6.778 13.75 8.228Z'/>"
        + "<path d='M1.25 7.498H10.94'/>"
        + "</g></svg>");

    private static readonly string CoffeeBreak = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#B8D0F0'/>"
        + "<g transform='translate(8,7) scale(1.85)' fill='none' stroke='white' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'>"
        + "<path d='M1.25 8.468V6.548C1.25 5.088 2.43 3.918 3.88 3.918H8.49C9.95 3.918 11.12 5.098 11.12 6.548V11.128C11.12 12.588 9.94 13.758 8.49 13.758H3.88C2.43 13.758 1.25 12.578 1.25 11.128'/>"
        + "<path d='M3.44 1.968V1.408'/>"
        + "<path d='M5.94 1.968V1.408'/>"
        + "<path d='M8.44 1.968V1.408'/>"
        + "<path d='M13.75 8.228C13.75 9.678 12.57 10.858 11.12 10.858V5.598C12.57 5.598 13.75 6.778 13.75 8.228Z'/>"
        + "<path d='M1.25 7.498H10.94'/>"
        + "</g></svg>");

    private static readonly string CoffeeDisabled = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#DDE3EC'/>"
        + "<g transform='translate(8,7) scale(1.85)' fill='none' stroke='white' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'>"
        + "<path d='M1.25 8.468V6.548C1.25 5.088 2.43 3.918 3.88 3.918H8.49C9.95 3.918 11.12 5.098 11.12 6.548V11.128C11.12 12.588 9.94 13.758 8.49 13.758H3.88C2.43 13.758 1.25 12.578 1.25 11.128'/>"
        + "<path d='M3.44 1.968V1.408'/>"
        + "<path d='M5.94 1.968V1.408'/>"
        + "<path d='M8.44 1.968V1.408'/>"
        + "<path d='M13.75 8.228C13.75 9.678 12.57 10.858 11.12 10.858V5.598C12.57 5.598 13.75 6.778 13.75 8.228Z'/>"
        + "<path d='M1.25 7.498H10.94'/>"
        + "</g></svg>");

    private static readonly string FoodBlue = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#5F96F9'/>"
        + "<g transform='translate(8,7) scale(1.85)' fill='none' stroke='white' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'>"
        + "<path d='M10.9615 14C10.3959 13.998 9.9389 13.538 9.941 12.972L9.956 8.875L12.005 8.882L11.99 12.98C11.988 13.545 11.527 14.002 10.9615 14Z'/>"
        + "<path d='M12.034 1.029L12.005 8.882L8.932 8.871L8.95 4.091C8.956 2.394 10.337 1.023 12.034 1.029Z'/>"
        + "<path d='M7.071 4.122L7.109 1.05'/>"
        + "<path d='M4.903 13.999C4.337 13.992 3.884 13.528 3.891 12.962L3.941 8.865L5.989 8.89L5.939 12.987C5.932 13.553 5.468 14.006 4.903 13.999Z'/>"
        + "<path d='M5.681 6.154L4.315 6.138C3.561 6.129 2.957 5.51 2.966 4.756L2.974 4.073L7.071 4.122L7.063 4.805C7.054 5.559 6.435 6.163 5.681 6.154Z'/>"
        + "<path d='M2.974 4.073L3.011 1'/>"
        + "<path d='M5.023 4.098L5.06 1.025'/>"
        + "<path d='M4.965 8.877L4.998 6.146'/>"
        + "</g></svg>");

    private static readonly string FoodBreak = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#B8D0F0'/>"
        + "<g transform='translate(8,7) scale(1.85)' fill='none' stroke='white' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'>"
        + "<path d='M10.9615 14C10.3959 13.998 9.9389 13.538 9.941 12.972L9.956 8.875L12.005 8.882L11.99 12.98C11.988 13.545 11.527 14.002 10.9615 14Z'/>"
        + "<path d='M12.034 1.029L12.005 8.882L8.932 8.871L8.95 4.091C8.956 2.394 10.337 1.023 12.034 1.029Z'/>"
        + "<path d='M7.071 4.122L7.109 1.05'/>"
        + "<path d='M4.903 13.999C4.337 13.992 3.884 13.528 3.891 12.962L3.941 8.865L5.989 8.89L5.939 12.987C5.932 13.553 5.468 14.006 4.903 13.999Z'/>"
        + "<path d='M5.681 6.154L4.315 6.138C3.561 6.129 2.957 5.51 2.966 4.756L2.974 4.073L7.071 4.122L7.063 4.805C7.054 5.559 6.435 6.163 5.681 6.154Z'/>"
        + "<path d='M2.974 4.073L3.011 1'/>"
        + "<path d='M5.023 4.098L5.06 1.025'/>"
        + "<path d='M4.965 8.877L4.998 6.146'/>"
        + "</g></svg>");

    private static readonly string FoodDisabled = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#DDE3EC'/>"
        + "<g transform='translate(8,7) scale(1.85)' fill='none' stroke='white' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'>"
        + "<path d='M10.9615 14C10.3959 13.998 9.9389 13.538 9.941 12.972L9.956 8.875L12.005 8.882L11.99 12.98C11.988 13.545 11.527 14.002 10.9615 14Z'/>"
        + "<path d='M12.034 1.029L12.005 8.882L8.932 8.871L8.95 4.091C8.956 2.394 10.337 1.023 12.034 1.029Z'/>"
        + "<path d='M7.071 4.122L7.109 1.05'/>"
        + "<path d='M4.903 13.999C4.337 13.992 3.884 13.528 3.891 12.962L3.941 8.865L5.989 8.89L5.939 12.987C5.932 13.553 5.468 14.006 4.903 13.999Z'/>"
        + "<path d='M5.681 6.154L4.315 6.138C3.561 6.129 2.957 5.51 2.966 4.756L2.974 4.073L7.071 4.122L7.063 4.805C7.054 5.559 6.435 6.163 5.681 6.154Z'/>"
        + "<path d='M2.974 4.073L3.011 1'/>"
        + "<path d='M5.023 4.098L5.06 1.025'/>"
        + "<path d='M4.965 8.877L4.998 6.146'/>"
        + "</g></svg>");

    private static readonly string OpenAppBlue = SvgDataUri(
        "<svg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'>"
        + "<rect width='44' height='44' rx='12' fill='#5F96F9'/>"
        + "<g transform='translate(10,10)' fill='none' stroke='white' stroke-width='2.1' stroke-linecap='round' stroke-linejoin='round'>"
        + "<path d='M14 5h5v5'/>"
        + "<path d='M10 14 19 5'/>"
        + "<path d='M19 14v4a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1V6a1 1 0 0 1 1-1h4'/>"
        + "</g></svg>");

    #endregion

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string? _compiledTemplate;

    // Every button is a single Image element (composite SVG with baked-in background).
    // selectAction on the parent Container handles the tap.
    // No ActionSet, no style:"positive", no backgroundImage — zero deformation.
    private static readonly string RawTemplate = """
    {
      "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
      "type": "AdaptiveCard",
      "version": "1.5",
      "padding": "None",
      "backgroundImage": {
        "url": "{{WhiteFillBg}}",
        "fillMode": "Cover"
      },
      "body": [
        {
          "type": "Container",
          "bleed": true,
          "style": "default",
          "items": [
            {
              "type": "TextBlock",
              "text": "${title}",
              "weight": "Bolder",
              "size": "Medium",
              "color": "Dark",
              "wrap": true
            },
            {
              "type": "TextBlock",
              "$when": "${isSignedIn}",
              "text": "${displayName}",
              "spacing": "Small",
              "weight": "Bolder",
              "color": "Dark",
              "wrap": true
            },
            {
              "type": "TextBlock",
              "$when": "${isSignedOut}",
              "text": "${message}",
              "color": "Dark",
              "wrap": true,
              "spacing": "Small"
            },
            {
              "type": "Container",
              "$when": "${isSignedOut}",
              "spacing": "Medium",
              "selectAction": { "type": "Action.Execute", "title": " ", "verb": "open-app" },
              "items": [
                {
                  "type": "Image",
                  "url": "{{OpenAppBlue}}",
                  "width": "44px",
                  "horizontalAlignment": "Center"
                }
              ]
            },
            {
              "type": "ColumnSet",
              "$when": "${isSignedIn}",
              "spacing": "Medium",
              "columns": [
                {
                  "type": "Column",
                  "width": "stretch",
                  "items": [
                    {
                      "type": "Container",
                      "backgroundImage": { "url": "{{WhiteCardBg}}", "fillMode": "Cover" },
                      "style": "default",
                      "items": [
                        {
                          "type": "TextBlock",
                          "text": "• ${sessionCounter}",
                          "size": "Large",
                          "weight": "Bolder",
                          "wrap": false,
                          "color": "Dark"
                        }
                      ]
                    }
                  ]
                },
                {
                  "type": "Column",
                  "width": "auto",
                  "items": [
                    {
                      "type": "Container",
                      "$when": "${showEntryButton}",
                      "selectAction": { "type": "Action.Execute", "title": " ", "verb": "clock-in" },
                      "items": [
                        { "type": "Image", "url": "{{EntryBlue}}", "width": "44px" }
                      ]
                    },
                    {
                      "type": "Container",
                      "$when": "${showClockOutButton}",
                      "selectAction": { "type": "Action.Execute", "title": " ", "verb": "clock-out" },
                      "items": [
                        { "type": "Image", "url": "{{StopBlue}}", "width": "44px" }
                      ]
                    },
                    {
                      "type": "Container",
                      "$when": "${showClockOutDisabled}",
                      "items": [
                        { "type": "Image", "url": "{{StopDisabled}}", "width": "44px" }
                      ]
                    }
                  ]
                },
                {
                  "type": "Column",
                  "width": "auto",
                  "items": [
                    {
                      "type": "Container",
                      "$when": "${showCoffeeActive}",
                      "selectAction": { "type": "Action.Execute", "title": " ", "verb": "${coffeeVerb}" },
                      "items": [
                        { "type": "Image", "url": "{{CoffeeBlue}}", "width": "44px" }
                      ]
                    },
                    {
                      "type": "Container",
                      "$when": "${showCoffeeEndBreak}",
                      "selectAction": { "type": "Action.Execute", "title": " ", "verb": "${coffeeVerb}" },
                      "items": [
                        { "type": "Image", "url": "{{CoffeeBreak}}", "width": "44px" }
                      ]
                    },
                    {
                      "type": "Container",
                      "$when": "${showCoffeeDisabled}",
                      "items": [
                        { "type": "Image", "url": "{{CoffeeDisabled}}", "width": "44px" }
                      ]
                    }
                  ]
                },
                {
                  "type": "Column",
                  "width": "auto",
                  "items": [
                    {
                      "type": "Container",
                      "$when": "${showFoodActive}",
                      "selectAction": { "type": "Action.Execute", "title": " ", "verb": "${foodVerb}" },
                      "items": [
                        { "type": "Image", "url": "{{FoodBlue}}", "width": "44px" }
                      ]
                    },
                    {
                      "type": "Container",
                      "$when": "${showFoodEndBreak}",
                      "selectAction": { "type": "Action.Execute", "title": " ", "verb": "${foodVerb}" },
                      "items": [
                        { "type": "Image", "url": "{{FoodBreak}}", "width": "44px" }
                      ]
                    },
                    {
                      "type": "Container",
                      "$when": "${showFoodDisabled}",
                      "items": [
                        { "type": "Image", "url": "{{FoodDisabled}}", "width": "44px" }
                      ]
                    }
                  ]
                }
              ]
            },
            {
              "type": "Container",
              "$when": "${isSignedIn}",
              "style": "default",
              "spacing": "Medium",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "${statusHeadline}",
                  "size": "Large",
                  "weight": "Bolder",
                  "color": "Dark",
                  "wrap": true
                },
                {
                  "type": "TextBlock",
                  "text": "${statusDetail}",
                  "color": "Dark",
                  "wrap": true,
                  "spacing": "Small",
                  "isSubtle": true
                }
              ]
            },
            {
              "type": "ColumnSet",
              "$when": "${isSignedIn}",
              "spacing": "Medium",
              "columns": [
                {
                  "type": "Column",
                  "width": "stretch",
                  "items": [
                    { "type": "TextBlock", "text": "Último fichaje", "color": "Dark", "isSubtle": true, "spacing": "None", "wrap": true },
                    { "type": "TextBlock", "text": "${lastCompletedShiftDuration}", "color": "Dark", "weight": "Bolder", "wrap": true }
                  ]
                },
                {
                  "type": "Column",
                  "width": "stretch",
                  "items": [
                    { "type": "TextBlock", "text": "Café hoy", "color": "Dark", "isSubtle": true, "spacing": "None", "wrap": true },
                    { "type": "TextBlock", "text": "${coffeeTodayDuration}", "color": "Dark", "weight": "Bolder", "wrap": true }
                  ]
                },
                {
                  "type": "Column",
                  "width": "stretch",
                  "items": [
                    { "type": "TextBlock", "text": "Comida hoy", "color": "Dark", "isSubtle": true, "spacing": "None", "wrap": true },
                    { "type": "TextBlock", "text": "${foodTodayDuration}", "color": "Dark", "weight": "Bolder", "wrap": true }
                  ]
                }
              ]
            },
            {
              "type": "Container",
              "$when": "${isSignedIn}",
              "spacing": "Medium",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Última acción: ${lastAction}",
                  "color": "Dark",
                  "wrap": true
                },
                {
                  "type": "TextBlock",
                  "text": "Hora: ${lastActionTime}",
                  "color": "Dark",
                  "spacing": "Small",
                  "wrap": true
                }
              ]
            },
            {
              "type": "TextBlock",
              "$when": "${isSignedIn && $host.widgetSize == \"large\"}",
              "text": "${timelineText}",
              "color": "Dark",
              "wrap": true,
              "spacing": "Medium",
              "isSubtle": true,
              "maxLines": 2
            }
          ]
        }
      ]
    }
    """;

    public static string BuildTemplate() =>
        _compiledTemplate ??= CompileTemplate();

    private static string CompileTemplate() =>
        RawTemplate
            .Replace("{{WhiteFillBg}}", WhiteFillBg)
            .Replace("{{WhiteCardBg}}", WhiteCardBg)
            .Replace("{{EntryBlue}}", EntryBlue)
            .Replace("{{EntryDisabled}}", EntryDisabled)
            .Replace("{{StopBlue}}", StopBlue)
            .Replace("{{StopDisabled}}", StopDisabled)
            .Replace("{{CoffeeBlue}}", CoffeeBlue)
            .Replace("{{CoffeeBreak}}", CoffeeBreak)
            .Replace("{{CoffeeDisabled}}", CoffeeDisabled)
            .Replace("{{FoodBlue}}", FoodBlue)
            .Replace("{{FoodBreak}}", FoodBreak)
            .Replace("{{FoodDisabled}}", FoodDisabled)
            .Replace("{{OpenAppBlue}}", OpenAppBlue);

    private static string SvgDataUri(string svg) =>
        "data:image/svg+xml," + Uri.EscapeDataString(svg);

    public static string BuildData(TimeTrackingWidgetViewModel viewModel)
    {
        var payload = viewModel switch
        {
            SignedOutWidgetViewModel signedOut => new
            {
                signedOut.Title,
                signedOut.CustomState,
                signedOut.SurfaceColorHex,
                signedOut.AccentColorHex,
                IsSignedOut = true,
                IsSignedIn = false,
                signedOut.Message,
                signedOut.PrimaryActionLabel,
                DisplayName = string.Empty,
                StatusHeadline = string.Empty,
                StatusDetail = string.Empty,
                SessionCounter = "00:00:00",
                LastAction = string.Empty,
                LastActionTime = string.Empty,
                LastCompletedShiftDuration = string.Empty,
                WorkedThisMonthDuration = string.Empty,
                CoffeeTodayDuration = string.Empty,
                FoodTodayDuration = string.Empty,
                TimelineText = string.Empty,
                CoffeeVerb = "start-coffee-break",
                FoodVerb = "start-food-break",
                ShowEntryButton = false,
                ShowClockOutButton = false,
                ShowClockOutDisabled = false,
                ShowCoffeeActive = false,
                ShowCoffeeEndBreak = false,
                ShowCoffeeDisabled = false,
                ShowFoodActive = false,
                ShowFoodEndBreak = false,
                ShowFoodDisabled = false
            },
            SignedInWidgetViewModel signedIn => new
            {
                signedIn.Title,
                signedIn.CustomState,
                signedIn.SurfaceColorHex,
                signedIn.AccentColorHex,
                IsSignedOut = false,
                IsSignedIn = true,
                Message = string.Empty,
                PrimaryActionLabel = string.Empty,
                signedIn.DisplayName,
                signedIn.StatusHeadline,
                signedIn.StatusDetail,
                signedIn.SessionCounter,
                signedIn.LastAction,
                signedIn.LastActionTime,
                signedIn.LastCompletedShiftDuration,
                signedIn.WorkedThisMonthDuration,
                signedIn.CoffeeTodayDuration,
                signedIn.FoodTodayDuration,
                signedIn.TimelineText,
                CoffeeVerb = signedIn.ActiveBreakType == BreakType.Coffee
                    ? "end-coffee-break" : "start-coffee-break",
                FoodVerb = signedIn.ActiveBreakType == BreakType.Food
                    ? "end-food-break" : "start-food-break",
                ShowEntryButton = signedIn.CanClockIn,
                ShowClockOutButton = signedIn.ActiveBreakType == BreakType.None && signedIn.CanClockOut,
                ShowClockOutDisabled = signedIn.ActiveBreakType != BreakType.None,
                ShowCoffeeActive = signedIn.ActiveBreakType == BreakType.None && signedIn.CanStartCoffeeBreak,
                ShowCoffeeEndBreak = signedIn.ActiveBreakType == BreakType.Coffee && signedIn.CanEndCoffeeBreak,
                ShowCoffeeDisabled = signedIn.ActiveBreakType == BreakType.Food
                    || (signedIn.ActiveBreakType == BreakType.None && !signedIn.CanStartCoffeeBreak)
                    || (signedIn.ActiveBreakType == BreakType.Coffee && !signedIn.CanEndCoffeeBreak),
                ShowFoodActive = signedIn.ActiveBreakType == BreakType.None && signedIn.CanStartFoodBreak,
                ShowFoodEndBreak = signedIn.ActiveBreakType == BreakType.Food && signedIn.CanEndFoodBreak,
                ShowFoodDisabled = signedIn.ActiveBreakType == BreakType.Coffee
                    || (signedIn.ActiveBreakType == BreakType.None && !signedIn.CanStartFoodBreak)
                    || (signedIn.ActiveBreakType == BreakType.Food && !signedIn.CanEndFoodBreak)
            },
            _ => throw new InvalidOperationException(
                $"Unsupported widget view model type: {viewModel.GetType().Name}")
        };

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }
}
