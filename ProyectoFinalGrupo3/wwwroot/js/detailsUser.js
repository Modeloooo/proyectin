$(document).ready(function () {

    $("#cerrarVer, #cerrarVerBtn").on("click", function () {
        $("#modalVer").addClass("hidden");
    });

    $("#modalVer").on("click", function (e) {
        if (e.target === this) {
            $(this).addClass("hidden");
        }
    });

    $("#edit-all").on("click", function () {
        // activar todos los inputs
        $("input , select").each(function () {
            $(this).removeAttr("disabled")
                .addClass("border-primary bg-white shadow-sm"); // estilo activo
        });

        // mostrar acciones de actualización
        $("#update-actions").removeClass("hidden");
    });

    // opcional: botón descartar → volver a deshabilitar
    $("#update-actions button:first").on("click", function () {
        $("input , select" ).each(function () {
            $(this).attr("disabled", true)
                .removeClass("border-primary bg-white shadow-sm"); // quitar estilo activo
        });

        $("#update-actions").addClass("hidden");
    });
});