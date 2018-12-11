
IMAGE = pullenti/pullenti-server

EP.SdkCore:
	cp -r ../PullentiNetCore/EP.SdkCore .

image: EP.SdkCore
	docker build -t $(IMAGE) .

push:
	docker push pullenti/pullenti-server

run:
	docker run -it --rm -p 8080:8080 $(IMAGE)
