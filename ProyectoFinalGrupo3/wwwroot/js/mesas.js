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

    // ABRIR MODAL EDITAR

    $(document).on("click", ".btnEditar", function () {

        let id = $(this).data("id");
        let capacidad = $(this).data("capacidad");
        let estado = $(this).data("estado");

        $("#editNumeroMesa").val(id);
        $("#editCapacidad").val(capacidad);
        $("#editEstado").val(estado);

        $("#modalEditar").removeClass("hidden");
    });

    // CERRAR MODAL
    $("#cerrarEditar, #cancelarEditar").on("click", function () {
        $("#modalEditar").addClass("hidden");
    });

    // cerrar al hacer click afuera
    $("#modalEditar").on("click", function (e) {
        if (e.target === this) {
            $(this).addClass("hidden");
        }
    });

    // ELIMINAR
    let modalEliminar = $("#modalEliminar");

    $(document).on("click", ".btnEliminar", function () {
        let id = $(this).data("id");

        $("#idEliminar").val(id);
        modalEliminar.removeClass("hidden");
    });

    $("#cancelarEliminar").on("click", function () {
        modalEliminar.addClass("hidden");
    });

    // cerrar al hacer click afuera
    modalEliminar.on("click", function (e) {
        if (e.target === this) {
            modalEliminar.addClass("hidden");
        }
    });

    // ABRIR MODAL VER
    $(document).on("click", ".btnVer", function () {

        let id = $(this).data("id");
        let capacidad = $(this).data("capacidad");
        let estado = $(this).data("estado");

        $("#verNumeroMesa").text("T-" + id);
        $("#verCapacidad").text(capacidad + " personas");
        $("#verEstado").text(estado);

        $("#modalVer").removeClass("hidden");
    });

    // CERRAR
    $("#cerrarVer, #btnCerrarVer").on("click", function () {
        $("#modalVer").addClass("hidden");
    });

    // cerrar al hacer click afuera
    $("#modalVer").on("click", function (e) {
        if (e.target === this) {
            $(this).addClass("hidden");
        }
    });
});
