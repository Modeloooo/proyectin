$(document).ready(function () {

    // Modal Agregar
    const $modalAgregar = $("#modalAgregar");

    $("#btnAgregar").on("click", function () {
        $modalAgregar.removeClass("hidden");
    });

    $("#btnCerrarModal, #btnCancelarModal").on("click", function () {
        $modalAgregar.addClass("hidden");
    });

    $modalAgregar.on("click", function (e) {
        if (e.target === this) {
            $modalAgregar.addClass("hidden");
        }
    });

    // Modal Editar
    $(document).on("click", ".btnEditar", function () {

        console.log("CLICK EDITAR");

        let id = $(this).data("id");
        let descripcion = $(this).data("descripcion");

        $("#editCodigo").val(id);
        $("#editDescripcion").val(descripcion);

        $("#modalEditar").removeClass("hidden");
    });

    $("#cerrarEditar, #cancelarEditar").on("click", function () {
        $("#modalEditar").addClass("hidden");
    });

    $("#modalEditar").on("click", function (e) {
        if (e.target === this) {
            $(this).addClass("hidden");
        }
    });

    // Modal Eliminar
    const modalEliminar = $("#modalEliminar");

    $(document).on("click", ".btnEliminar", function () {
        let id = $(this).data("id");

        $("#idEliminar").val(id);
        modalEliminar.removeClass("hidden");
    });

    $("#cancelarEliminar").on("click", function () {
        modalEliminar.addClass("hidden");
    });

    modalEliminar.on("click", function (e) {
        if (e.target === this) {
            modalEliminar.addClass("hidden");
        }
    });

    // Modal Ver
    $(document).on("click", ".btnVer", function () {

        let id = $(this).data("id");
        let descripcion = $(this).data("descripcion");

        $("#verCodigo").text("CAT#" + id);
        $("#verDescripcion").text(descripcion);

        $("#modalVer").removeClass("hidden");
    });

    $("#cerrarVer, #cerrarVerBtn").on("click", function () {
        $("#modalVer").addClass("hidden");
    });

    $("#modalVer").on("click", function (e) {
        if (e.target === this) {
            $(this).addClass("hidden");
        }
    });
});