var modelsArray = ["3d/demo/shirt/scene.gltf", "3d/demo/t-shirt/scene.gltf"];

(function () {
    function renderModel(wrapper, modelSource) {
        if (wrapper == null || modelSource == null) return;
        //let wrapper = document.querySelector('.model-wrapper');
        if (wrapper == null) return;
        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(
            40,  // fov
            1280 / 720,   // aspect
            0.05, // near
            500); // far
        camera.position.z = 2.1;
        camera.position.y = 2.7;
        camera.position.x = 0;
        const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });

        renderer.setClearColor(0x000000, 0);
        renderer.setSize(640, 360);
        wrapper.appendChild(renderer.domElement);

        const pLight = new THREE.PointLight(0xFFFFFF, 3);
        const pLight2 = new THREE.PointLight(0xFFFFFF, 3);
        const pLight3 = new THREE.PointLight(0xFFFFFF, 3);
        const pLight4 = new THREE.PointLight(0xFFFFFF, 3);
        pLight.position.z = 1;
        pLight.position.y = 3;
        pLight.position.x = 0;
        pLight2.position.z = -1;
        pLight2.position.y = 3;
        pLight2.position.x = 0;

        pLight3.position.z = 0;
        pLight3.position.y = 3;
        pLight3.position.x = 2;

        pLight4.position.z = 0;
        pLight4.position.y = 3;
        pLight4.position.x = -2;

        scene.add(pLight3);
        scene.add(pLight4);
        scene.add(pLight2);
        scene.add(pLight);

        let loader = new THREE.GLTFLoader();
        let obj = null;
        loader.load(modelSource, function (gltf) {
            obj = gltf;
            obj.scene.scale.set(2, 2, 2);
            obj.scene.position.z = 0;
            obj.scene.position.y = 0;
            obj.scene.position.x = 0;
            scene.add(obj.scene);
            var controls = new ObjectControls(camera, renderer.domElement, obj.scene);
            controls.disableZoom();
            controls.setRotationSpeed(.15);
            controls.setRotationSpeedTouchDevices(.1);
        });
        function animate() {
            requestAnimationFrame(animate);
            renderer.render(scene, camera);
        };
        animate();
    }

    (function () {
        $('.model-wrapper').each((i, e) => { renderModel(e, modelsArray[0]) });
    })();
})();

/*
- controls.setDistance(8, 200); // sets the min - max distance able to zoom
- controls.setZoomSpeed(1); // sets the zoom speed ( 0.1 == slow, 1 == fast)
- controls.disableZoom(); // disables zoom
- controls.enableZoom(); // enables zoom
- controls.setObjectToMove(newMesh); // changes the object to interact with
- controls.setObjectToMove([mshBox, mshBox2]); // changes the objects to interact with
- controls.setRotationSpeed(0.05); // sets a new rotation speed for desktop ( 0.1 == slow, 1 == fast)
- controls.setRotationSpeedTouchDevices(value); // sets a new rotation speed for mobile
- controls.enableVerticalRotation(); // enables the vertical rotation
- constrols.disableVerticalRotation();  // disables the vertical rotation
- controls.enableHorizontalRotation(); // enables the horizontal rotation
- controls.disableHorizontalRotation();// disables the horizontal rotation
- controls.setMaxVerticalRotationAngle(Math.PI / 4, Math.PI / 4); // sets a max angle value for the vertical rotation of the object
- controls.setMaxHorizontalRotationAngle(R, R); // sets a max angle value for the horizontal rotation of the object
- controls.disableMaxHorizontalAngleRotation(); // disables angle limits for horizontal rotation
- controls.disableMaxVerticalAngleRotation(); // disables angle limits for vertical rotation
- controls.isUserInteractionActive() //returns true if the user is interacting with the UI, false otherwise
*/