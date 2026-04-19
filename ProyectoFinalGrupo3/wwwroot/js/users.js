$(document).ready(function () {

    function abrirModal(modalId) {
        $(modalId).removeClass("hidden");
    }

    function cerrarModal(modalId) {
        $(modalId).addClass("hidden");
    }

    // MODAL AGREGAR
    $("#btnAgregar").on("click", function () {
        abrirModal("#modalAgregar");
    });

    $("#btnCerrarModal, #btnCancelarModal").on("click", function () {
        cerrarModal("#modalAgregar");
    });

    $("#modalAgregar").on("click", function (e) {
        if (e.target === this) {
            cerrarModal("#modalAgregar");
        }
    });

    // MODAL EDITAR
    $(document).on("click", ".btnEditar", function () {
        let identificacion = $(this).data("identificacion");
        let nombrecompleto = $(this).data("nombrecompleto");
        let genero = $(this).data("genero");
        let correo = $(this).data("correo");
        let tipotarjeta = $(this).data("tipotarjeta");
        let numerotarjeta = $(this).data("numerotarjeta");
        let perfil = $(this).data("perfil");

        $("#editIdentificacion").val(identificacion);
        $("#editNombreCompleto").val(nombrecompleto);
        $("#editGenero").val(genero);
        $("#editCorreo").val(correo);
        $("#editTipoTarjeta").val(tipotarjeta);
        $("#editNumeroTarjeta").val(numerotarjeta);
        $("#editPerfil").val(perfil);
        $("#editContrasena").val("");

        abrirModal("#modalEditar");
    });

    $("#cerrarEditar, #cancelarEditar").on("click", function () {
        cerrarModal("#modalEditar");
    });

    $("#modalEditar").on("click", function (e) {
        if (e.target === this) {
            cerrarModal("#modalEditar");
        }
    });

    // MODAL ELIMINAR
    $(document).on("click", ".btnEliminar", function () {
        let id = $(this).data("id");
        $("#idEliminar").val(id);
        abrirModal("#modalEliminar");
    });

    $("#cancelarEliminar").on("click", function () {
        cerrarModal("#modalEliminar");
    });

    $("#modalEliminar").on("click", function (e) {
        if (e.target === this) {
            cerrarModal("#modalEliminar");
        }
    });

    // MODAL VER
    $(document).on("click", ".btnVer", function () {
        let identificacion = $(this).data("identificacion");
        let nombrecompleto = $(this).data("nombrecompleto");
        let genero = $(this).data("genero");
        let correo = $(this).data("correo");
        let tipotarjeta = $(this).data("tipotarjeta");
        let numerotarjeta = $(this).data("numerotarjeta");
        let dinerodisponible = $(this).data("dinerodisponible");
        let perfil = $(this).data("perfil");

        $("#verIdentificacion").text(identificacion || "-");
        $("#verNombreCompleto").text(nombrecompleto || "-");
        $("#verGenero").text(genero || "-");
        $("#verCorreo").text(correo || "-");
        $("#verTipoTarjeta").text(tipotarjeta || "-");
        $("#verNumeroTarjeta").text(numerotarjeta || "-");
        $("#verDineroDisponible").text("₡" + (dinerodisponible ?? 0));
        $("#verPerfil").text(perfil || "-");

        abrirModal("#modalVer");
    });

    $("#cerrarVer, #btnCerrarVer").on("click", function () {
        cerrarModal("#modalVer");
    });

    $("#modalVer").on("click", function (e) {
        if (e.target === this) {
            cerrarModal("#modalVer");
        }
    });

});